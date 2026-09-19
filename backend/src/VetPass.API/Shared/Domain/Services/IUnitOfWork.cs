namespace VetPass.API.Shared.Domain.Services;

/// <summary>
/// Closes a use case as a single transaction. The generation of a vaccination
/// card, for instance, must persist together with the registration of the pet
/// that originates it (US09-E1).
/// </summary>
public interface IUnitOfWork
{
    Task CompleteAsync(CancellationToken cancellationToken = default);
}
