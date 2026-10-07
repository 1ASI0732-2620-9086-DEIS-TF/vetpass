using VetPass.API.IAM.Application.Internal.CommandServices;
using VetPass.API.IAM.Domain.Model.Aggregates;
using VetPass.API.IAM.Domain.Model.ValueObjects;
using VetPass.API.IAM.Interfaces.REST.Resources;

namespace VetPass.API.IAM.Interfaces.REST.Transform;

public static class UserResourceFromEntityAssembler
{
    public static UserResource ToResource(UserProfile profile, string? clinicName = null) => new(
        profile.Id,
        profile.Email,
        profile.FullName,
        profile.Role.ToClaimValue(),
        profile.ClinicId,
        profile.ClientId,
        clinicName,
        profile.RequiresPasswordChange);
}

public static class AuthenticatedSessionResourceFromEntityAssembler
{
    public static AuthenticatedSessionResource ToResource(AuthenticatedSession session) => new(
        session.Token.Value,
        session.Token.RefreshToken,
        session.Token.ExpiresInSeconds,
        "Bearer",
        UserResourceFromEntityAssembler.ToResource(session.Profile, session.ClinicName));
}

public static class CreatedAccountResourceFromEntityAssembler
{
    public static CreatedAccountResource ToResource(CreatedAccount account) => new(
        UserResourceFromEntityAssembler.ToResource(account.Profile),
        account.TemporaryPassword);
}
