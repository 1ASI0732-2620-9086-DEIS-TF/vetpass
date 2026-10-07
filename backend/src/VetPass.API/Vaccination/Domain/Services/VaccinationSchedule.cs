using VetPass.API.Patients.Domain.Model.ValueObjects;
using VetPass.API.Shared.Domain.Exceptions;
using VetPass.API.Vaccination.Domain.Model.Entities;
using VetPass.API.Vaccination.Domain.Model.ValueObjects;

namespace VetPass.API.Vaccination.Domain.Services;

/// <summary>
/// Template of the schedule of a species, and the policy that validates it.
/// This is the only component of the design whose behaviour depends on rules
/// external to the data itself, which is why it lives as a domain service and
/// not inside the aggregate.
/// </summary>
public class VaccinationSchedule
{
    private readonly List<ScheduleItem> _items;

    public Species Species { get; }
    public IReadOnlyList<ScheduleItem> Items => _items.AsReadOnly();

    public VaccinationSchedule(Species species, IEnumerable<ScheduleItem> items)
    {
        Species = species;
        _items = items
            .Where(item => item.Species == species)
            .OrderBy(item => item.MinimumAgeInWeeks)
            .ThenBy(item => item.SequenceNumber)
            .ToList();

        if (_items.Count == 0)
            throw new EmptyScheduleException(species);
    }

    public IReadOnlyList<ScheduleItem> ItemsFor(Species species) =>
        _items.Where(item => item.Species == species).ToList().AsReadOnly();

    /// <summary>
    /// Date on which a dose is expected when the card is generated: the birth
    /// date of the pet plus the minimum age of the vaccine.
    /// </summary>
    public DateOnly ExpectedDateFor(ScheduleItem item, DateOnly birthDate) =>
        birthDate.AddDays(item.MinimumAgeInWeeks * 7);

    /// <summary>
    /// Verifies that the pet reaches the minimum age of the vaccine on the date
    /// of application (US10-E2).
    /// </summary>
    /// <summary>
    /// First day on which the dose may be applied: the minimum age of the
    /// vaccine and, from the second dose on, the minimum interval since the
    /// previous application. It is not the expected date, which never falls
    /// before the day the plan is made; a dose given years ago is admissible
    /// on its real date even though the plan would place it today.
    /// </summary>
    public static DateOnly EarliestAdmissibleDate(Dose dose, Dose? previous, DateOnly birthDate)
    {
        var byAge = birthDate.AddDays(dose.MinimumAgeInWeeks * 7);
        if (previous?.ApplicationDate is null) return byAge;

        var byInterval = previous.ApplicationDate.Value.AddDays(dose.MinimumIntervalInWeeks * 7);
        return byInterval > byAge ? byInterval : byAge;
    }

    public static void EnsureMinimumAge(Dose dose, DateOnly birthDate, DateOnly applicationDate)
    {
        var earliestAdmissibleDate = birthDate.AddDays(dose.MinimumAgeInWeeks * 7);
        if (applicationDate >= earliestAdmissibleDate) return;

        var ageInWeeks = (applicationDate.DayNumber - birthDate.DayNumber) / 7;
        throw new MinimumAgeNotReachedException(ageInWeeks, dose.MinimumAgeInWeeks, earliestAdmissibleDate);
    }

    /// <summary>
    /// Verifies that the minimum interval since the previous applied dose of the
    /// same vaccine has elapsed (US10-E3).
    /// </summary>
    public static void EnsureMinimumInterval(Dose dose, Dose? previous, DateOnly applicationDate)
    {
        if (previous?.ApplicationDate is null || dose.MinimumIntervalInWeeks <= 0) return;

        var earliestAdmissibleDate = previous.ApplicationDate.Value.AddDays(dose.MinimumIntervalInWeeks * 7);
        if (applicationDate >= earliestAdmissibleDate) return;

        var elapsedWeeks = (applicationDate.DayNumber - previous.ApplicationDate.Value.DayNumber) / 7;
        throw new MinimumIntervalNotMetException(
            elapsedWeeks, dose.MinimumIntervalInWeeks, earliestAdmissibleDate);
    }
}

public class MinimumAgeNotReachedException(int ageInWeeks, int requiredWeeks, DateOnly earliestAdmissibleDate)
    : DomainRuleViolationException(
        $"Edad mínima no alcanzada. La mascota tiene {ageInWeeks} semanas y la vacuna exige un mínimo de " +
        $"{requiredWeeks} semanas. La fecha más temprana admisible es el {earliestAdmissibleDate:dd/MM/yyyy}.")
{
    public override string Code => "minimum-age-not-reached";
    public int AgeInWeeks { get; } = ageInWeeks;
    public int RequiredWeeks { get; } = requiredWeeks;
    public DateOnly EarliestAdmissibleDate { get; } = earliestAdmissibleDate;
}

public class MinimumIntervalNotMetException(int elapsedWeeks, int requiredWeeks, DateOnly earliestAdmissibleDate)
    : DomainRuleViolationException(
        $"Intervalo mínimo no cumplido. Transcurrieron {elapsedWeeks} semanas desde la dosis anterior y el " +
        $"esquema exige {requiredWeeks}. La fecha más temprana admisible es el {earliestAdmissibleDate:dd/MM/yyyy}.")
{
    public override string Code => "minimum-interval-not-met";
    public int ElapsedWeeks { get; } = elapsedWeeks;
    public int RequiredWeeks { get; } = requiredWeeks;
    public DateOnly EarliestAdmissibleDate { get; } = earliestAdmissibleDate;
}

public class EmptyScheduleException(Species species)
    : InvalidDomainDataException($"No existe un esquema de vacunación definido para la especie {species}.")
{
    public override string Code => "empty-schedule";
}
