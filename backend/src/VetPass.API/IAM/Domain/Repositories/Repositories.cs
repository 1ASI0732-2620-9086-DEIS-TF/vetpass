using VetPass.API.IAM.Domain.Model.Aggregates;

namespace VetPass.API.IAM.Domain.Repositories;

public interface IUserProfileRepository
{
    Task<UserProfile?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserProfile?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(UserProfile profile, CancellationToken cancellationToken = default);
}

public interface IClinicRepository
{
    Task<Clinic?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Clinic?> FindFirstAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Clinic clinic, CancellationToken cancellationToken = default);
}
