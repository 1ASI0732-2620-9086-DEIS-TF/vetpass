using VetPass.API.Patients.Domain.Model.ValueObjects;
using VetPass.API.Shared.Domain.Exceptions;
using VetPass.API.Vaccination.Domain.Model.Entities;
using VetPass.API.Vaccination.Domain.Model.ValueObjects;
using VetPass.API.Vaccination.Domain.Services;

namespace VetPass.API.Vaccination.Domain.Model.Aggregates;

/// <summary>
/// Vaccination card of a pet: aggregate root of the Vaccination bounded
/// context and the place where the complexity of the domain is concentrated.
///
/// The collection of doses is never exposed for modification. A dose becomes
/// applied only through <see cref="RegisterDose"/>, which checks every rule of
/// the schedule before altering the internal state, so that a card cannot be
/// built into an invalid state.
///
/// The current date always enters as a parameter and is never read from the
/// system clock, so that the behaviour of the card is deterministic and its
/// rules can be verified independently of when the verification runs.
/// </summary>
public class VaccinationCard
{
    private readonly List<Dose> _doses = [];

    public Guid Id { get; private set; }
    public Guid PetId { get; private set; }
    public Species Species { get; private set; }
    public DateOnly PetBirthDate { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<Dose> Doses => _doses.AsReadOnly();

    // Required by Entity Framework Core.
    private VaccinationCard() { }

    private VaccinationCard(Guid petId, Species species, DateOnly birthDate)
    {
        Id = Guid.NewGuid();
        PetId = petId;
        Species = species;
        PetBirthDate = birthDate;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Generates the card of a pet from the template of its species (US09-E1).
    /// Every item of the schedule becomes a pending dose whose expected date is
    /// already calculated from the birth date of the pet (US09-E2).
    /// </summary>
    public static VaccinationCard GenerateFrom(Guid petId, VaccinationSchedule schedule, DateOnly birthDate)
    {
        var card = new VaccinationCard(petId, schedule.Species, birthDate);

        foreach (var item in schedule.Items)
            card._doses.Add(new Dose(card.Id, item, schedule.ExpectedDateFor(item, birthDate)));

        return card;
    }

    /// <summary>
    /// Records the application of a dose (US10). The order of the checks is the
    /// order of the acceptance criteria: a dose already applied, a date in the
    /// future, the minimum age of the vaccine and the interval since the
    /// previous dose of the same vaccine.
    /// </summary>
    public void RegisterDose(Guid doseId, DateOnly applicationDate, BatchCode batchCode,
        Guid veterinarianId, DateOnly today)
    {
        var dose = _doses.FirstOrDefault(d => d.Id == doseId)
                   ?? throw new ResourceNotFoundException("una dosis de esta cartilla", doseId);

        if (dose.IsApplied())
            throw new DoseAlreadyAppliedException(dose.Id, dose.ApplicationDate!.Value);

        if (applicationDate > today)
            throw new FutureApplicationDateException(applicationDate, today);

        VaccinationSchedule.EnsureMinimumAge(dose, PetBirthDate, applicationDate);
        VaccinationSchedule.EnsureMinimumInterval(dose, PreviousAppliedDose(dose), applicationDate);

        dose.MarkAsApplied(applicationDate, batchCode, veterinarianId);

        RescheduleFollowingDose(dose);
    }

    /// <summary>
    /// Status of the card at a given date (US11).
    ///
    /// <para><b>Overdue</b> when at least one dose was expected on an earlier
    /// date and has not been applied. <b>Pending</b> when a dose falls due
    /// today and is still awaiting its application. <b>Up to date</b> when
    /// every dose whose expected date has already arrived is applied, which is
    /// the reading of US11-E1: doses expected in the future — the annual
    /// boosters, for instance — do not by themselves make a card pending.</para>
    /// </summary>
    public CardStatus GetStatus(DateOnly today)
    {
        if (_doses.Any(dose => dose.IsOverdue(today)))
            return CardStatus.Overdue;

        if (_doses.Any(dose => !dose.IsApplied() && dose.ExpectedDate == today))
            return CardStatus.Pending;

        return CardStatus.UpToDate;
    }

    /// <summary>The next dose awaiting application, the one the interface announces.</summary>
    public Dose? NextExpectedDose() => _doses
        .Where(dose => !dose.IsApplied())
        .OrderBy(dose => dose.ExpectedDate)
        .ThenBy(dose => dose.SequenceNumber)
        .FirstOrDefault();

    /// <summary>
    /// The last applied dose of the same vaccine preceding the one being
    /// registered, which is the one the interval is measured from.
    /// </summary>
    private Dose? PreviousAppliedDose(Dose dose) => _doses
        .Where(d => d.VaccineId == dose.VaccineId
                    && d.SequenceNumber < dose.SequenceNumber
                    && d.IsApplied())
        .OrderByDescending(d => d.SequenceNumber)
        .FirstOrDefault();

    /// <summary>
    /// Moves the next pending dose of the same vaccine when the one just
    /// applied shifts it: its expected date becomes the later of the minimum
    /// age and the minimum interval since this application (rule 2 of SP01).
    /// </summary>
    private void RescheduleFollowingDose(Dose applied)
    {
        var following = _doses
            .Where(d => d.VaccineId == applied.VaccineId
                        && d.SequenceNumber > applied.SequenceNumber
                        && !d.IsApplied())
            .OrderBy(d => d.SequenceNumber)
            .FirstOrDefault();

        if (following is null) return;

        var byMinimumAge = PetBirthDate.AddDays(following.MinimumAgeInWeeks * 7);
        var byMinimumInterval = applied.ApplicationDate!.Value.AddDays(following.MinimumIntervalInWeeks * 7);

        following.Reschedule(byMinimumAge > byMinimumInterval ? byMinimumAge : byMinimumInterval);
    }
}

public class FutureApplicationDateException(DateOnly applicationDate, DateOnly today)
    : DomainRuleViolationException(
        $"La fecha de aplicación {applicationDate:dd/MM/yyyy} es posterior a la fecha actual {today:dd/MM/yyyy}.")
{
    public override string Code => "future-application-date";
}
