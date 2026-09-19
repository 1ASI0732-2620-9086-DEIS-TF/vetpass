using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using VetPass.API.IAM.Domain.Model.Aggregates;
using VetPass.API.IAM.Domain.Model.ValueObjects;
using VetPass.API.IAM.Domain.Services;
using VetPass.API.Shared.Domain.Exceptions;

namespace VetPass.API.IAM.Infrastructure.Identity;

/// <summary>
/// Adapter towards Supabase Auth. It is the only place in the API that knows
/// how the provider is spoken to: which endpoint issues a token, which header
/// carries the key and how the role is recorded on an account.
/// </summary>
public class SupabaseAuthGateway(HttpClient http, IOptions<SupabaseAuthOptions> options,
    ILogger<SupabaseAuthGateway> logger) : IIdentityProvider
{
    private readonly SupabaseAuthOptions _options = options.Value;

    public async Task<AccessToken> SignInAsync(string email, string password,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"{_options.AuthUrl}/token?grant_type=password")
        {
            Content = JsonContent.Create(new { email, password })
        };
        request.Headers.Add("apikey", _options.AnonKey);

        using var response = await http.SendAsync(request, cancellationToken);

        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
            throw new InvalidCredentialsException();

        return await ReadTokenAsync(response, cancellationToken);
    }

    public async Task<AccessToken> RefreshAsync(string refreshToken,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"{_options.AuthUrl}/token?grant_type=refresh_token")
        {
            Content = JsonContent.Create(new { refresh_token = refreshToken })
        };
        request.Headers.Add("apikey", _options.AnonKey);

        using var response = await http.SendAsync(request, cancellationToken);

        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
            throw new InvalidCredentialsException();

        return await ReadTokenAsync(response, cancellationToken);
    }

    public async Task<IdentityAccount> CreateAccountAsync(string email, string password, Role role,
        Guid? clinicId, Guid? clientId, CancellationToken cancellationToken = default)
    {
        // The role and the membership travel in app_metadata, which the user
        // cannot modify: that is what makes the claim trustworthy when the API
        // reads it back from the token.
        var payload = new Dictionary<string, object?>
        {
            ["email"] = email,
            ["password"] = password,
            ["email_confirm"] = true,
            ["app_metadata"] = new Dictionary<string, object?>
            {
                ["role"] = role.ToClaimValue(),
                ["clinic_id"] = clinicId?.ToString(),
                ["client_id"] = clientId?.ToString()
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.AuthUrl}/admin/users")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("apikey", _options.ServiceRoleKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ServiceRoleKey);

        using var response = await http.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("El proveedor de identidad rechazó la creación de la cuenta {Email}: {Status} {Body}",
                email, (int)response.StatusCode, body);
            throw new AccountCreationFailedException(email);
        }

        using var document = JsonDocument.Parse(body);
        var id = document.RootElement.GetProperty("id").GetString();

        return new IdentityAccount(Guid.Parse(id!), email);
    }

    private static async Task<AccessToken> ReadTokenAsync(HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;

        return new AccessToken(
            root.GetProperty("access_token").GetString()!,
            root.TryGetProperty("refresh_token", out var refresh) ? refresh.GetString() ?? string.Empty : string.Empty,
            root.TryGetProperty("expires_in", out var expires) ? expires.GetInt32() : 3600);
    }
}

public class AccountCreationFailedException(string email)
    : InvalidDomainDataException($"No fue posible crear la cuenta de acceso para {email}.")
{
    public override string Code => "account-creation-failed";
}
