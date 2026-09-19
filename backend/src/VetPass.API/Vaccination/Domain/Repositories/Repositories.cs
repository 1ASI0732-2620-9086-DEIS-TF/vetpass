using VetPass.API.Patients.Domain.Model.ValueObjects;
using VetPass.API.Vaccination.Domain.Model.Aggregates;
using VetPass.API.Vaccination.Domain.Model.Entities;
using VetPass.API.Vaccination.Domain.Model.ValueObjects;

namespace VetPass.API.Vaccination.Domain.Repositories;

public interface IVaccinationCardRepository
{
    Task<VaccinationCard?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VaccinationCard?> FindByPetIdAsync(Guid petId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VaccinationCard>> ListByPetIdsAsync(IEnumerable<Guid> petIds,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsForPetAsync(Guid petId, CancellationToken cancellationToken = default);
    Task AddAsync(VaccinationCard card, CancellationToken cancellationToken = default);
}

/// <summary>
/// Catalogue of vaccines and template of the schedule. Both are read-only for
/// the application: their content comes from the SP01 spike and is loaded by
/// the seed of the platform.
/// </summary>
public interface IVaccineCatalogRepository
{
    Task<IReadOnlyList<ScheduleItem>> ListScheduleItemsAsync(Species species,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vaccine>> ListVaccinesAsync(CancellationToken cancellationToken = default);
}
