namespace VetPass.API.Vaccination.Domain.Model.ValueObjects;

/// <summary>
/// Overall condition of a pet's vaccination schedule at a given date (US11).
/// The three values are the ones the interface shows as a labelled chip with
/// its own colour and icon.
/// </summary>
public enum CardStatus
{
    UpToDate = 1,
    Pending = 2,
    Overdue = 3
}

/// <summary>Condition of an individual dose within a card.</summary>
public enum DoseStatus
{
    Pending = 1,
    Applied = 2
}
