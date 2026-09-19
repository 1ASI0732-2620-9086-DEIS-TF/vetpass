using Microsoft.EntityFrameworkCore;
using VetPass.API.MedicalRecords.Domain.Model.Aggregates;
using VetPass.API.MedicalRecords.Domain.Repositories;
using VetPass.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using VetPass.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace VetPass.API.MedicalRecords.Infrastructure.Persistence.EFC.Repositories;

public class VisitRepository(VetPassDbContext context)
    : BaseRepository<Visit>(context), IVisitRepository
{
    public async Task<Visit?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await WithPrescription().FirstOrDefaultAsync(visit => visit.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Visit>> ListByPetIdAsync(Guid petId,
        CancellationToken cancellationToken = default) =>
        await WithPrescription()
            .Where(visit => visit.PetId == petId)
            .OrderByDescending(visit => visit.VisitDate)
            .ThenByDescending(visit => visit.CreatedAt)
            .ToListAsync(cancellationToken);

    private IQueryable<Visit> WithPrescription() =>
        Context.Visits
            .Include(visit => visit.Prescription)
            .ThenInclude(prescription => prescription!.Items);
}
