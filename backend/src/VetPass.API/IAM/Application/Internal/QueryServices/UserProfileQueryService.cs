using VetPass.API.IAM.Domain.Model.Aggregates;
using VetPass.API.IAM.Domain.Repositories;
using VetPass.API.Shared.Domain.Exceptions;

namespace VetPass.API.IAM.Application.Internal.QueryServices;

public class UserProfileQueryService(IUserProfileRepository userProfiles)
{
    public async Task<UserProfile> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await userProfiles.FindByIdAsync(id, cancellationToken)
        ?? throw new ResourceNotFoundException("un perfil de usuario", id);

    public async Task<UserProfile?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await userProfiles.FindByIdAsync(id, cancellationToken);
}
