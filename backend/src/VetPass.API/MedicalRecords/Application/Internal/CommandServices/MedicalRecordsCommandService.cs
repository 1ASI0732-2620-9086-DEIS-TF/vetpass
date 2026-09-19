using VetPass.API.MedicalRecords.Domain.Model.Aggregates;
using VetPass.API.MedicalRecords.Domain.Model.Commands;
using VetPass.API.MedicalRecords.Domain.Model.ValueObjects;
using VetPass.API.MedicalRecords.Domain.Repositories;
using VetPass.API.Shared.Domain.Exceptions;
using VetPass.API.Shared.Domain.Services;

namespace VetPass.API.MedicalRecords.Application.Internal.CommandServices;

public class MedicalRecordsCommandService(
    IVisitRepository visits,
    IClinicClock clock,
    IUnitOfWork unitOfWork)
{
    /// <summary>Registers a visit in the history of a patient (US13).</summary>
    public async Task<Visit> RegisterVisitAsync(RegisterVisitCommand command,
        CancellationToken cancellationToken = default)
    {
        var visit = new Visit(command.PetId, command.VeterinarianId, command.VisitDate, command.Reason,
            command.Findings, command.Diagnosis, command.Treatment, command.WeightKg, clock.Today);

        await visits.AddAsync(visit, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return visit;
    }

    /// <summary>
    /// Issues the prescription of a visit, which becomes visible to the owner of
    /// the pet in the mobile application (US15).
    /// </summary>
    public async Task<Visit> IssuePrescriptionAsync(IssuePrescriptionCommand command,
        CancellationToken cancellationToken = default)
    {
        var visit = await visits.FindByIdAsync(command.VisitId, cancellationToken)
                    ?? throw new ResourceNotFoundException("una atención", command.VisitId);

        visit.IssuePrescription(command.Items
            .Select(item => new PrescriptionItem(item.Medication, item.Dosage, item.Duration)));

        await unitOfWork.CompleteAsync(cancellationToken);
        return visit;
    }
}
