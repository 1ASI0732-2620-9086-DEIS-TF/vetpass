namespace VetPass.API.MedicalRecords.Domain.Model.Commands;

public record RegisterVisitCommand(Guid PetId, Guid VeterinarianId, DateOnly VisitDate, string Reason,
    string? Findings, string Diagnosis, string? Treatment, decimal? WeightKg);

public record PrescriptionItemCommand(string Medication, string Dosage, string Duration);

public record IssuePrescriptionCommand(Guid VisitId, IReadOnlyList<PrescriptionItemCommand> Items);
