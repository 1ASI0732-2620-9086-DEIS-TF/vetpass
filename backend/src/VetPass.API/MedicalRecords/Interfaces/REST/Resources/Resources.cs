using System.ComponentModel.DataAnnotations;

namespace VetPass.API.MedicalRecords.Interfaces.REST.Resources;

public record RegisterVisitResource(
    [Required] DateOnly VisitDate,
    [Required] string Reason,
    string? Findings,
    [Required] string Diagnosis,
    string? Treatment,
    decimal? WeightKg,
    Guid? VeterinarianId,
    IReadOnlyList<PrescriptionItemResource>? Prescription);

public record PrescriptionItemResource(
    [Required] string Medication,
    [Required] string Dosage,
    [Required] string Duration);

public record IssuePrescriptionResource(
    [Required, MinLength(1)] IReadOnlyList<PrescriptionItemResource> Items);

public record PrescriptionResource(Guid Id, DateTime IssuedAt, IEnumerable<PrescriptionItemResource> Items);

public record VisitResource(
    Guid Id,
    Guid PetId,
    Guid VeterinarianId,
    DateOnly VisitDate,
    string Reason,
    string? Findings,
    string Diagnosis,
    string? Treatment,
    decimal? WeightKg,
    bool HasPrescription,
    PrescriptionResource? Prescription);
