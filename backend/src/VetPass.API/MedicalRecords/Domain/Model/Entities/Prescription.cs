using VetPass.API.MedicalRecords.Domain.Model.ValueObjects;
using VetPass.API.Shared.Domain.Exceptions;

namespace VetPass.API.MedicalRecords.Domain.Model.Entities;

/// <summary>
/// Prescription issued during a visit. It cannot exist without at least one
/// medication (US15-E2), so its items are given at construction time.
///
/// Neither the prescription nor its items assign their own identifier: they are
/// not aggregate roots, and leaving the key to the persistence layer is what
/// lets them be added to a visit that was already saved. An entity that arrives
/// with its key already set is indistinguishable, for the change tracker, from
/// one that already exists in the database.
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

        VisitId = visitId;
        IssuedAt = DateTime.UtcNow;

        foreach (var item in received)
            AddItem(item);
    }

    internal void AddItem(PrescriptionItem item) => _items.Add(item);
}

public class EmptyPrescriptionException()
    : InvalidDomainDataException("Una receta no puede emitirse sin al menos un medicamento.")
{
    public override string Code => "empty-prescription";
}
