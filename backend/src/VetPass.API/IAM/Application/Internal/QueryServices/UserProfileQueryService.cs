using VetPass.API.IAM.Domain.Model.Aggregates;
using VetPass.API.IAM.Domain.Repositories;
using VetPass.API.Shared.Domain.Exceptions;

namespace VetPass.API.IAM.Application.Internal.QueryServices;

/// <summary>Perfil del usuario junto con la clínica a la que pertenece.</summary>
public record UserProfileView(UserProfile Profile, string? ClinicName);

public class UserProfileQueryService(IUserProfileRepository userProfiles, IClinicRepository clinics)
{
    public async Task<UserProfileView> GetViewByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var profile = await GetByIdAsync(id, cancellationToken);
        var clinica = profile.ClinicId is null
            ? null
            : await clinics.FindByIdAsync(profile.ClinicId.Value, cancellationToken);

        return new UserProfileView(profile, clinica?.Name);
    }

    public async Task<UserProfile> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await userProfiles.FindByIdAsync(id, cancellationToken)
        ?? throw new ResourceNotFoundException("un perfil de usuario", id);

    public async Task<UserProfile?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await userProfiles.FindByIdAsync(id, cancellationToken);
}
