using VetPass.API.IAM.Domain.Model.ValueObjects;

namespace VetPass.API.IAM.Domain.Services;

/// <summary>Access token issued by the identity provider.</summary>
public record AccessToken(string Value, string RefreshToken, int ExpiresInSeconds);

/// <summary>Account of a user as the identity provider knows it.</summary>
public record IdentityAccount(Guid Id, string Email);

/// <summary>
/// Port towards the identity provider. The application layer talks to this
/// interface and knows nothing about the provider behind it; the adapter that
/// implements it is the Supabase Auth Gateway, in the infrastructure layer.
/// </summary>
public interface IIdentityProvider
{
    Task<AccessToken> SignInAsync(string email, string password, CancellationToken cancellationToken = default);

    Task<AccessToken> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the account of a user and records in it the role and the
    /// membership the domain assigns, so that they travel inside every token
    /// the provider issues for that account.
    /// </summary>
    Task<IdentityAccount> CreateAccountAsync(string email, string password, Role role,
        Guid? clinicId, Guid? clientId, CancellationToken cancellationToken = default);

    /// <summary>Replaces the password of an account.</summary>
    Task SetPasswordAsync(Guid accountId, string password, CancellationToken cancellationToken = default);
}
