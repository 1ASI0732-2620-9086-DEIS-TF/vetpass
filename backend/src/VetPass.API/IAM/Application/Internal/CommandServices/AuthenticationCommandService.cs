using System.Security.Cryptography;
using VetPass.API.IAM.Domain.Model.Aggregates;
using VetPass.API.IAM.Domain.Model.Commands;
using VetPass.API.IAM.Domain.Model.ValueObjects;
using VetPass.API.IAM.Domain.Repositories;
using VetPass.API.IAM.Domain.Services;
using VetPass.API.Shared.Domain.Services;

namespace VetPass.API.IAM.Application.Internal.CommandServices;

/// <summary>Session opened by a user, with the profile the domain recognises.</summary>
public record AuthenticatedSession(AccessToken Token, UserProfile Profile);

/// <summary>
/// Account just created. The temporary password is returned once and is not
/// stored anywhere: the reception desk hands it to the owner of the pet, who
/// changes it afterwards (US05).
/// </summary>
public record CreatedAccount(UserProfile Profile, string TemporaryPassword);

public class AuthenticationCommandService(
    IIdentityProvider identityProvider,
    IUserProfileRepository userProfiles,
    IUnitOfWork unitOfWork)
{
    public async Task<AuthenticatedSession> SignInAsync(SignInCommand command,
        CancellationToken cancellationToken = default)
    {
        var token = await identityProvider.SignInAsync(command.Email, command.Password, cancellationToken);

        // An account of the provider without a profile in the platform is not a
        // user of VetPass: it is denied as if the credentials did not match,
        // without revealing which of the two things failed.
        var profile = await userProfiles.FindByEmailAsync(command.Email, cancellationToken)
                      ?? throw new InvalidCredentialsException();

        return new AuthenticatedSession(token, profile);
    }

    public async Task<AccessToken> RefreshAsync(RefreshSessionCommand command,
        CancellationToken cancellationToken = default) =>
        await identityProvider.RefreshAsync(command.RefreshToken, cancellationToken);

    /// <summary>
    /// Creates the account of a user and its profile. The account is created
    /// first: if the provider rejects it, nothing is written in the database.
    /// </summary>
    public async Task<CreatedAccount> CreateAccountAsync(CreateAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        if (await userProfiles.ExistsByEmailAsync(command.Email, cancellationToken))
            throw new EmailAlreadyRegisteredException(command.Email);

        var temporaryPassword = GenerateTemporaryPassword();

        var account = await identityProvider.CreateAccountAsync(command.Email, temporaryPassword,
            command.Role, command.ClinicId, command.ClientId, cancellationToken);

        var profile = new UserProfile(account.Id, command.Email, command.FullName,
            command.Role, command.ClinicId, command.ClientId);

        await userProfiles.AddAsync(profile, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return new CreatedAccount(profile, temporaryPassword);
    }

    /// <summary>
    /// Readable password, long enough to be safe and short enough to be dictated
    /// at the reception desk. Characters that are confused when read aloud —
    /// zero and O, one and l — are left out.
    /// </summary>
    private static string GenerateTemporaryPassword()
    {
        const string alphabet = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
        var characters = RandomNumberGenerator.GetItems<char>(alphabet, 12);
        return new string(characters);
    }
}

public class EmailAlreadyRegisteredException(string email)
    : Shared.Domain.Exceptions.InvalidDomainDataException(
        $"El correo {email} ya está registrado en la plataforma.")
{
    public override string Code => "email-already-registered";
}
