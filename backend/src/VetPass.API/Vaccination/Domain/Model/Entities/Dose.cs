using VetPass.API.Shared.Domain.Exceptions;
using VetPass.API.Vaccination.Domain.Model.ValueObjects;

namespace VetPass.API.Vaccination.Domain.Model.Entities;

/// <summary>
/// One dose of the schedule of a card. It is created pending, with its expected
/// date already calculated, and becomes applied only through the card, which is
/// the aggregate root that guards the rules.
/// </summary>
public class Dose
{
    public Guid Id { get; private set; }
    public Guid CardId { get; private set; }
    public Guid VaccineId { get; private set; }
    public int SequenceNumber { get; private set; }
    public DateOnly ExpectedDate { get; private set; }
    public DateOnly? ApplicationDate { get; private set; }
    public BatchCode? BatchCode { get; private set; }
    public Guid? VeterinarianId { get; private set; }
    public DoseStatus Status { get; private set; }

    /// <summary>
    /// Rules of the schedule item, copied when the card was generated. Keeping
    /// them on the dose is what makes the card preserve the schedule in force
    /// at the time of its creation.
    /// </summary>
    public int MinimumAgeInWeeks { get; private set; }

    public int MinimumIntervalInWeeks { get; private set; }

    // Required by Entity Framework Core.
    private Dose() { }

    public Dose(Guid cardId, ScheduleItem item, DateOnly expectedDate)
    {
        Id = Guid.NewGuid();
        CardId = cardId;
        VaccineId = item.VaccineId;
        SequenceNumber = item.SequenceNumber;
        ExpectedDate = expectedDate;
        Status = DoseStatus.Pending;
        MinimumAgeInWeeks = item.MinimumAgeInWeeks;
        MinimumIntervalInWeeks = item.MinimumIntervalInWeeks;
    }

    public bool IsApplied() => Status == DoseStatus.Applied;

    /// <summary>
    /// A dose is overdue when it has not been applied and the date on which it
    /// was expected has already passed (US11-E3).
    /// </summary>
    public bool IsOverdue(DateOnly today) => !IsApplied() && ExpectedDate < today;

    internal void MarkAsApplied(DateOnly date, BatchCode batchCode, Guid veterinarianId)
    {
        if (IsApplied())
            throw new DoseAlreadyAppliedException(Id, ApplicationDate!.Value);

        ApplicationDate = date;
        BatchCode = batchCode;
        VeterinarianId = veterinarianId;
        Status = DoseStatus.Applied;
    }

    /// <summary>
    /// Moves the expected date of a pending dose. Used when the preceding dose
    /// of the same vaccine is applied late: the schedule shifts instead of
    /// carrying a date that can no longer be met (rule 2 of SP01).
    /// </summary>
    internal void Reschedule(DateOnly expectedDate)
    {
        if (IsApplied()) return;
        ExpectedDate = expectedDate;
    }
}

public class DoseAlreadyAppliedException(Guid doseId, DateOnly applicationDate)
    : DomainRuleViolationException(
        $"La dosis {doseId} ya fue registrada como aplicada el {applicationDate:dd/MM/yyyy}.")
{
    public override string Code => "dose-already-applied";
}
