using VetPass.API.MedicalRecords.Domain.Model.Aggregates;
using VetPass.API.MedicalRecords.Domain.Repositories;
using VetPass.API.Shared.Domain.Exceptions;

namespace VetPass.API.MedicalRecords.Application.Internal.QueryServices;

public class MedicalRecordsQueryService(IVisitRepository visits)
{
    public async Task<IReadOnlyList<Visit>> ListByPetIdAsync(Guid petId,
        CancellationToken cancellationToken = default) =>
        await visits.ListByPetIdAsync(petId, cancellationToken);

    public async Task<Visit> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await visits.FindByIdAsync(id, cancellationToken)
        ?? throw new ResourceNotFoundException("una atención", id);
}
