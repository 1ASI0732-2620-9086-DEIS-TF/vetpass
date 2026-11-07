using System.Collections.Concurrent;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using VetPass.API.IAM.Domain.Model.Aggregates;
using VetPass.API.IAM.Domain.Model.ValueObjects;
using VetPass.API.IAM.Domain.Services;

namespace VetPass.Testing;

/// <summary>
/// Identity provider in memory, in place of Supabase Auth. It issues the same
/// kind of token Supabase does —issuer, audience and app_metadata— signed with
/// the test secret, so the API validates it exactly as in production.
/// </summary>
public sealed class FakeIdentityProvider(TimeProvider clock) : IIdentityProvider
{
    private sealed record Account(Guid Id, string Email, string Password, Role Role, Guid? ClinicId, Guid? ClientId);

    private readonly ConcurrentDictionary<string, Account> _accounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, string> _refreshTokens = new();

    public void Clear()
    {
        _accounts.Clear();
        _refreshTokens.Clear();
    }

    public Task<AccessToken> SignInAsync(string email, string password, CancellationToken cancellationToken = default) =>
        _accounts.TryGetValue(email.Trim(), out var account) && account.Password == password
            ? Task.FromResult(Issue(account))
            : throw new InvalidCredentialsException();

    public Task<AccessToken> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default) =>
        _refreshTokens.TryRemove(refreshToken, out var email) && _accounts.TryGetValue(email, out var account)
            ? Task.FromResult(Issue(account))
            : throw new InvalidCredentialsException();

    public Task<IdentityAccount> CreateAccountAsync(string email, string password, Role role, Guid? clinicId,
        Guid? clientId, CancellationToken cancellationToken = default)
    {
        var account = new Account(Guid.NewGuid(), email.Trim().ToLowerInvariant(), password, role, clinicId, clientId);
        _accounts[account.Email] = account;
        return Task.FromResult(new IdentityAccount(account.Id, account.Email));
    }

    public Task SetPasswordAsync(Guid accountId, string password, CancellationToken cancellationToken = default)
    {
        var account = _accounts.Values.Single(a => a.Id == accountId);
        _accounts[account.Email] = account with { Password = password };
        return Task.CompletedTask;
    }

    /// <summary>A token signed with another key, which the API must reject.</summary>
    public static string ForgedToken() => Sign(new Dictionary<string, object> { ["sub"] = Guid.NewGuid().ToString() },
        DateTime.UtcNow, "not-the-secret-of-the-api-but-long-enough!");

    private AccessToken Issue(Account account)
    {
        var refresh = Guid.NewGuid().ToString("N");
        _refreshTokens[refresh] = account.Email;

        var claims = new Dictionary<string, object>
        {
            ["sub"] = account.Id.ToString(),
            ["email"] = account.Email,
            ["role"] = "authenticated",
            ["app_metadata"] = new Dictionary<string, object?>
            {
                ["role"] = account.Role.ToClaimValue(),
                ["clinic_id"] = account.ClinicId?.ToString(),
                ["client_id"] = account.ClientId?.ToString(),
            },
        };

        return new AccessToken(Sign(claims, clock.GetUtcNow().UtcDateTime, VetPassApiFactory.JwtSecret), refresh, 3600);
    }

    private static string Sign(Dictionary<string, object> claims, DateTime now, string secret)
    {
        // The token is valid both at the test clock and at the real clock, since
        // the API validates its lifetime against the latter.
        var real = DateTime.UtcNow;
        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = $"{VetPassApiFactory.SupabaseUrl}/auth/v1",
            Audience = "authenticated",
            Claims = claims,
            IssuedAt = (now < real ? now : real).AddMinutes(-1),
            NotBefore = (now < real ? now : real).AddMinutes(-1),
            Expires = (now > real ? now : real).AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)), SecurityAlgorithms.HmacSha256),
        });
    }
}
