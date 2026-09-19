using VetPass.API.MedicalRecords.Domain.Model.Aggregates;
using VetPass.API.MedicalRecords.Interfaces.REST.Resources;

namespace VetPass.API.MedicalRecords.Interfaces.REST.Transform;

public static class VisitResourceFromEntityAssembler
{
    public static VisitResource ToResource(Visit visit) => new(
        visit.Id,
        visit.PetId,
        visit.VeterinarianId,
        visit.VisitDate,
        visit.Reason,
        visit.Findings,
        visit.Diagnosis,
        visit.Treatment,
        visit.WeightKg,
        visit.HasPrescription(),
        visit.Prescription is null
            ? null
            : new PrescriptionResource(
                visit.Prescription.Id,
                visit.Prescription.IssuedAt,
                visit.Prescription.Items.Select(item =>
                    new PrescriptionItemResource(item.Medication, item.Dosage, item.Duration))));
}
