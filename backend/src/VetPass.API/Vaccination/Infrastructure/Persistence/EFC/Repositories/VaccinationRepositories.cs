using Microsoft.EntityFrameworkCore;
using VetPass.API.Patients.Domain.Model.ValueObjects;
using VetPass.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using VetPass.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using VetPass.API.Vaccination.Domain.Model.Aggregates;
using VetPass.API.Vaccination.Domain.Model.Entities;
using VetPass.API.Vaccination.Domain.Model.ValueObjects;
using VetPass.API.Vaccination.Domain.Repositories;
using VetPass.API.Vaccination.Domain.Services;

namespace VetPass.API.Vaccination.Infrastructure.Persistence.EFC.Repositories;

public class VaccinationCardRepository(VetPassDbContext context)
    : BaseRepository<VaccinationCard>(context), IVaccinationCardRepository
{
    public async Task<VaccinationCard?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await WithDoses().FirstOrDefaultAsync(card => card.Id == id, cancellationToken);

    public async Task<VaccinationCard?> FindByPetIdAsync(Guid petId,
        CancellationToken cancellationToken = default) =>
        await WithDoses().FirstOrDefaultAsync(card => card.PetId == petId, cancellationToken);

    public async Task<IReadOnlyList<VaccinationCard>> ListByPetIdsAsync(IEnumerable<Guid> petIds,
        CancellationToken cancellationToken = default)
    {
        var ids = petIds.ToList();
        return await WithDoses()
            .Where(card => ids.Contains(card.PetId))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsForPetAsync(Guid petId, CancellationToken cancellationToken = default) =>
        await Context.VaccinationCards.AnyAsync(card => card.PetId == petId, cancellationToken);

    private IQueryable<VaccinationCard> WithDoses() =>
        Context.VaccinationCards.Include(card => card.Doses);
}

public class VaccineCatalogRepository(VetPassDbContext context) : IVaccineCatalogRepository
{
    public async Task<IReadOnlyList<ScheduleItem>> ListScheduleItemsAsync(Species species,
        CancellationToken cancellationToken = default) =>
        await context.ScheduleItems
            .Where(item => item.Species == species)
            .OrderBy(item => item.MinimumAgeInWeeks)
            .ThenBy(item => item.SequenceNumber)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Vaccine>> ListVaccinesAsync(CancellationToken cancellationToken = default) =>
        await context.Vaccines.OrderBy(vaccine => vaccine.Name).ToListAsync(cancellationToken);
}

/// <summary>
/// Assembles the schedule of a species from the template stored in the
/// database, which is the one the SP01 spike documents.
/// </summary>
public class VaccinationScheduleProvider(IVaccineCatalogRepository catalog) : IVaccinationScheduleProvider
{
    public async Task<VaccinationSchedule> GetForAsync(Species species,
        CancellationToken cancellationToken = default)
    {
        var items = await catalog.ListScheduleItemsAsync(species, cancellationToken);
        return new VaccinationSchedule(species, items);
    }
}
