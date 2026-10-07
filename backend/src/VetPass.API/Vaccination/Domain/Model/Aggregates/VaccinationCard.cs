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
    /// already calculated (US09-E2).
    ///
    /// The plan starts on the day the pet is registered: a dose whose minimum
    /// age has long passed is expected today, not on the date it would have
    /// been due years ago, and the following doses of the series keep their
    /// intervals from there (US09-E3). That is what "catching up" means for an
    /// adult pet or one with an unknown history.
    /// </summary>
    public static VaccinationCard GenerateFrom(Guid petId, VaccinationSchedule schedule, DateOnly birthDate,
        DateOnly today)
    {
        var card = new VaccinationCard(petId, schedule.Species, birthDate);

        foreach (var item in schedule.Items)
            card._doses.Add(new Dose(card.Id, item, schedule.ExpectedDateFor(item, birthDate)));

        foreach (var vaccineId in card._doses.Select(dose => dose.VaccineId).Distinct().ToList())
            card.Replan(vaccineId, today);

        return card;
    }

    /// <summary>
    /// Records the application of a dose (US10). The order of the checks is the
    /// order of the acceptance criteria: a dose already applied, a date in the
    /// future, the previous doses of the same vaccine, the minimum age of the
    /// vaccine and the interval since the previous dose.
    ///
    /// Recording a dose replans the rest of its own series from today. Other
    /// vaccines are left as they are, so a dose nobody applied keeps showing
    /// as overdue while the days go by (US11-E3).
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

        // Each series is applied in order: the second dose of a vaccine cannot
        // be recorded before the first one (US10-E5). Different vaccines are
        // independent, so the third multivalent dose and the first rabies dose
        // can still be recorded on the same day.
        var missing = FirstMissingPreviousDose(dose);
        if (missing is not null)
            throw new DoseOutOfOrderException(missing.Id, missing.SequenceNumber);

        VaccinationSchedule.EnsureMinimumAge(dose, PetBirthDate, applicationDate);
        VaccinationSchedule.EnsureMinimumInterval(dose, PreviousAppliedDose(dose), applicationDate);

        dose.MarkAsApplied(applicationDate, batchCode, veterinarianId);

        Replan(dose.VaccineId, today);
    }

    /// <summary>
    /// Whether the dose is the one its series is waiting for: pending, with
    /// every previous dose of the same vaccine already applied. The interfaces
    /// ask this instead of repeating the rule.
    /// </summary>
    public bool IsNextInSequence(Dose dose) =>
        !dose.IsApplied() && FirstMissingPreviousDose(dose) is null;

    /// <summary>
    /// First day on which the dose could be recorded, for the dose its series
    /// is waiting for; none for the others. The interfaces check the date the
    /// user enters against it before sending, without repeating the rule.
    /// </summary>
    public DateOnly? EarliestAdmissibleDate(Dose dose) => IsNextInSequence(dose)
        ? VaccinationSchedule.EarliestAdmissibleDate(dose, PreviousAppliedDose(dose), PetBirthDate)
        : null;

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

    /// <summary>The first earlier dose of the same vaccine still awaiting application.</summary>
    private Dose? FirstMissingPreviousDose(Dose dose) => _doses
        .Where(d => d.VaccineId == dose.VaccineId
                    && d.SequenceNumber < dose.SequenceNumber
                    && !d.IsApplied())
        .OrderBy(d => d.SequenceNumber)
        .FirstOrDefault();

    /// <summary>
    /// Plans the pending doses of one series, in order (rules 1 and 2 of SP01).
    /// Each one is expected on the latest of three dates: the minimum age of
    /// the vaccine, the minimum interval since the previous dose of the series
    /// —applied or planned— and today. The last one is what keeps the plan
    /// from pointing to dates that already went by.
    /// </summary>
    private void Replan(Guid vaccineId, DateOnly today)
    {
        DateOnly? previous = null;

        foreach (var dose in _doses.Where(d => d.VaccineId == vaccineId).OrderBy(d => d.SequenceNumber))
        {
            if (dose.IsApplied())
            {
                previous = dose.ApplicationDate;
                continue;
            }

            var expected = PetBirthDate.AddDays(dose.MinimumAgeInWeeks * 7);

            if (previous is not null)
                expected = Later(expected, previous.Value.AddDays(dose.MinimumIntervalInWeeks * 7));

            expected = Later(expected, today);

            dose.Reschedule(expected);
            previous = expected;
        }
    }

    private static DateOnly Later(DateOnly first, DateOnly second) => first > second ? first : second;
}

public class FutureApplicationDateException(DateOnly applicationDate, DateOnly today)
    : DomainRuleViolationException(
        $"La fecha de aplicación {applicationDate:dd/MM/yyyy} es posterior a la fecha actual {today:dd/MM/yyyy}.")
{
    public override string Code => "future-application-date";
}

public class DoseOutOfOrderException(Guid pendingDoseId, int pendingSequenceNumber)
    : DomainRuleViolationException(
        $"Antes de esta dosis debe registrarse la dosis {pendingSequenceNumber} de la misma vacuna: " +
        "cada serie se aplica en orden.")
{
    public override string Code => "dose-out-of-order";
    public Guid PendingDoseId { get; } = pendingDoseId;
    public int PendingSequenceNumber { get; } = pendingSequenceNumber;
}
