using VetPass.API.IAM.Domain.Model.Aggregates;

namespace VetPass.API.IAM.Domain.Repositories;

public interface IUserProfileRepository
{
    Task<UserProfile?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserProfile?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clientes de la clínica que ya tienen acceso a la aplicación móvil. La
    /// interfaz lo necesita para distinguir a quién le falta entregárselo.
    /// </summary>
    Task<IReadOnlyList<Guid>> ListClientIdsWithAccountAsync(Guid clinicId,
        CancellationToken cancellationToken = default);
    /// <summary>The mobile account of a client of the clinic, if it has one.</summary>
    Task<UserProfile?> FindByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);

    Task AddAsync(UserProfile profile, CancellationToken cancellationToken = default);
}

public interface IClinicRepository
{
    Task<Clinic?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Clinic?> FindFirstAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Clinic clinic, CancellationToken cancellationToken = default);
}
