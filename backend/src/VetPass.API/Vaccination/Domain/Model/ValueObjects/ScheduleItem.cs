using VetPass.API.Patients.Domain.Model.ValueObjects;

namespace VetPass.API.Vaccination.Domain.Model.ValueObjects;

/// <summary>
/// One dose within the template of a species. Read-only for the application:
/// its rows are materialised as doses when a card is generated, so that a later
/// change of the template never alters cards already issued (section 4.10).
/// The values come from the SP01 spike, in docs/spikes.
/// </summary>
public class ScheduleItem
{
    public Guid Id { get; private set; }
    public Guid VaccineId { get; private set; }
    public Species Species { get; private set; }
    public int SequenceNumber { get; private set; }
    public int MinimumAgeInWeeks { get; private set; }
    public int MinimumIntervalInWeeks { get; private set; }

    // Required by Entity Framework Core.
    private ScheduleItem() { }

    public ScheduleItem(Guid id, Guid vaccineId, Species species, int sequenceNumber,
        int minimumAgeInWeeks, int minimumIntervalInWeeks)
    {
        Id = id;
        VaccineId = vaccineId;
        Species = species;
        SequenceNumber = sequenceNumber;
        MinimumAgeInWeeks = minimumAgeInWeeks;
        MinimumIntervalInWeeks = minimumIntervalInWeeks;
    }

    /// <summary>True when this item continues a sequence started by a previous dose.</summary>
    public bool HasPrecedingDose => SequenceNumber > 1 && MinimumIntervalInWeeks > 0;
}
