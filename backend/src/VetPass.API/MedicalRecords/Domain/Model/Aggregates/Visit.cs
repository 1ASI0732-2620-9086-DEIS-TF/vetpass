using VetPass.API.MedicalRecords.Domain.Model.Entities;
using VetPass.API.MedicalRecords.Domain.Model.ValueObjects;
using VetPass.API.Shared.Domain.Exceptions;

namespace VetPass.API.MedicalRecords.Domain.Model.Aggregates;

/// <summary>
/// A veterinary visit: aggregate root of the Medical Records bounded context.
/// It groups in one place the diagnosis, the treatment and the prescription
/// issued that day, which is what the label <i>Atención</i> announces in the
/// interface.
/// </summary>
public class Visit
{
    public Guid Id { get; private set; }
    public Guid PetId { get; private set; }
    public Guid VeterinarianId { get; private set; }
    public DateOnly VisitDate { get; private set; }
    public string Reason { get; private set; } = null!;
    public string? Findings { get; private set; }
    public string Diagnosis { get; private set; } = null!;
    public string? Treatment { get; private set; }
    public decimal? WeightKg { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Prescription? Prescription { get; private set; }

    // Required by Entity Framework Core.
    private Visit() { }

    public Visit(Guid petId, Guid veterinarianId, DateOnly visitDate, string reason, string? findings,
        string diagnosis, string? treatment, decimal? weightKg, DateOnly today)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new RequiredVisitFieldException("motivo de consulta");

        if (string.IsNullOrWhiteSpace(diagnosis))
            throw new RequiredVisitFieldException("diagnóstico");

        if (visitDate > today)
            throw new FutureVisitDateException(visitDate, today);

        Id = Guid.NewGuid();
        PetId = petId;
        VeterinarianId = veterinarianId;
        VisitDate = visitDate;
        Reason = reason.Trim();
        Findings = string.IsNullOrWhiteSpace(findings) ? null : findings.Trim();
        Diagnosis = diagnosis.Trim();
        Treatment = string.IsNullOrWhiteSpace(treatment) ? null : treatment.Trim();
        WeightKg = weightKg;
        CreatedAt = DateTime.UtcNow;
    }

    public bool HasPrescription() => Prescription is not null;

    /// <summary>Issues the prescription of this visit (US15-E1).</summary>
    public void IssuePrescription(IEnumerable<PrescriptionItem> items)
    {
        if (HasPrescription())
            throw new PrescriptionAlreadyIssuedException(Id);

        Prescription = new Prescription(Id, items);
    }
}

public class RequiredVisitFieldException(string field)
    : InvalidDomainDataException($"El campo '{field}' de la atención es obligatorio.")
{
    public override string Code => "required-visit-field";
}

public class FutureVisitDateException(DateOnly visitDate, DateOnly today)
    : InvalidDomainDataException(
        $"La fecha de la atención {visitDate:dd/MM/yyyy} es posterior a la fecha actual {today:dd/MM/yyyy}.")
{
    public override string Code => "future-visit-date";
}

public class PrescriptionAlreadyIssuedException(Guid visitId)
    : DomainRuleViolationException($"La atención {visitId} ya tiene una receta emitida.")
{
    public override string Code => "prescription-already-issued";
}
