using VetPass.API.Shared.Domain.Exceptions;

namespace VetPass.API.MedicalRecords.Domain.Model.ValueObjects;

/// <summary>
/// A medication indicated in a prescription, with its dosage and its duration.
/// </summary>
public class PrescriptionItem
{
    public Guid Id { get; private set; }
    public Guid PrescriptionId { get; private set; }
    public string Medication { get; private set; } = null!;
    public string Dosage { get; private set; } = null!;
    public string Duration { get; private set; } = null!;

    // Required by Entity Framework Core.
    private PrescriptionItem() { }

    public PrescriptionItem(string medication, string dosage, string duration)
    {
        if (string.IsNullOrWhiteSpace(medication))
            throw new RequiredPrescriptionFieldException("medicamento");
        if (string.IsNullOrWhiteSpace(dosage))
            throw new RequiredPrescriptionFieldException("dosificación");
        if (string.IsNullOrWhiteSpace(duration))
            throw new RequiredPrescriptionFieldException("duración");

        Medication = medication.Trim();
        Dosage = dosage.Trim();
        Duration = duration.Trim();
    }
}

public class RequiredPrescriptionFieldException(string field)
    : InvalidDomainDataException($"El campo '{field}' de la receta es obligatorio.")
{
    public override string Code => "required-prescription-field";
}
