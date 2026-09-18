using VetPass.API.MedicalRecords.Domain.Model.ValueObjects;
using VetPass.API.Shared.Domain.Exceptions;

namespace VetPass.API.MedicalRecords.Domain.Model.Entities;

/// <summary>
/// Prescription issued during a visit. It cannot exist without at least one
/// medication (US15-E2), so its items are given at construction time.
/// </summary>
public class Prescription
{
    private readonly List<PrescriptionItem> _items = [];

    public Guid Id { get; private set; }
    public Guid VisitId { get; private set; }
    public DateTime IssuedAt { get; private set; }

    public IReadOnlyCollection<PrescriptionItem> Items => _items.AsReadOnly();

    // Required by Entity Framework Core.
    private Prescription() { }

    internal Prescription(Guid visitId, IEnumerable<PrescriptionItem> items)
    {
        var received = items.ToList();
        if (received.Count == 0)
            throw new EmptyPrescriptionException();

        Id = Guid.NewGuid();
        VisitId = visitId;
        IssuedAt = DateTime.UtcNow;

        foreach (var item in received)
            AddItem(item);
    }

    internal void AddItem(PrescriptionItem item)
    {
        item.AttachTo(Id);
        _items.Add(item);
    }
}

public class EmptyPrescriptionException()
    : InvalidDomainDataException("Una receta no puede emitirse sin al menos un medicamento.")
{
    public override string Code => "empty-prescription";
}
