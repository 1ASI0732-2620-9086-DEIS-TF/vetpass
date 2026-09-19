using VetPass.API.MedicalRecords.Domain.Model.Aggregates;

namespace VetPass.API.MedicalRecords.Domain.Repositories;

public interface IVisitRepository
{
    Task<Visit?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// History of a pet in descending chronological order: the most recent
    /// visit is the one of greatest clinical relevance and heads the listing
    /// (US14-E1, section 4.2.1).
    /// </summary>
    Task<IReadOnlyList<Visit>> ListByPetIdAsync(Guid petId, CancellationToken cancellationToken = default);

    Task AddAsync(Visit visit, CancellationToken cancellationToken = default);
}
