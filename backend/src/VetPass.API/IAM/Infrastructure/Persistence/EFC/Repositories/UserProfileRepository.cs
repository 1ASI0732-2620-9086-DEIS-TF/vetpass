using Microsoft.EntityFrameworkCore;
using VetPass.API.IAM.Domain.Model.Aggregates;
using VetPass.API.IAM.Domain.Repositories;
using VetPass.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using VetPass.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace VetPass.API.IAM.Infrastructure.Persistence.EFC.Repositories;

public class UserProfileRepository(VetPassDbContext context)
    : BaseRepository<UserProfile>(context), IUserProfileRepository
{
    public async Task<UserProfile?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Context.UserProfiles.FirstOrDefaultAsync(profile => profile.Id == id, cancellationToken);

    public async Task<UserProfile?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await Context.UserProfiles
            .FirstOrDefaultAsync(profile => profile.Email == email.Trim().ToLower(), cancellationToken);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await Context.UserProfiles.AnyAsync(profile => profile.Email == email.Trim().ToLower(), cancellationToken);

    public async Task<IReadOnlyList<Guid>> ListClientIdsWithAccountAsync(Guid clinicId,
        CancellationToken cancellationToken = default) =>
        await Context.UserProfiles
            .Where(profile => profile.ClinicId == clinicId && profile.ClientId != null)
            .Select(profile => profile.ClientId!.Value)
            .ToListAsync(cancellationToken);
}

public class ClinicRepository(VetPassDbContext context)
    : BaseRepository<Clinic>(context), IClinicRepository
{
    public async Task<Clinic?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Context.Clinics.FirstOrDefaultAsync(clinic => clinic.Id == id, cancellationToken);

    public async Task<Clinic?> FindFirstAsync(CancellationToken cancellationToken = default) =>
        await Context.Clinics.OrderBy(clinic => clinic.Name).FirstOrDefaultAsync(cancellationToken);
}
