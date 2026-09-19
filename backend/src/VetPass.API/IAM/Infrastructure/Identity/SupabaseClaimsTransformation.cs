using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using VetPass.API.Shared.Interfaces.ASP.Security;

namespace VetPass.API.IAM.Infrastructure.Identity;

/// <summary>
/// Translates the token of the provider into the terms of the domain.
///
/// Supabase records the role and the membership of a user inside the
/// <c>app_metadata</c> claim, as a nested object that the user cannot alter.
/// This transformation unpacks it into flat claims, so that the rest of the API
/// reasons about a role of the platform and never about the shape of a token.
/// </summary>
public class SupabaseClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity { IsAuthenticated: true } identity)
            return Task.FromResult(principal);

        if (identity.HasClaim(claim => claim.Type == CurrentUser.RoleClaim))
            return Task.FromResult(principal);

        var metadata = identity.FindFirst("app_metadata")?.Value;
        if (string.IsNullOrWhiteSpace(metadata)) return Task.FromResult(principal);

        try
        {
            using var document = JsonDocument.Parse(metadata);
            var root = document.RootElement;

            AddClaim(identity, root, "role", CurrentUser.RoleClaim);
            AddClaim(identity, root, "clinic_id", CurrentUser.ClinicClaim);
            AddClaim(identity, root, "client_id", CurrentUser.ClientClaim);

            // The role also becomes a role claim of ASP.NET, so that the
            // authorisation policies of the controllers can rely on it.
            var role = root.TryGetProperty("role", out var value) ? value.GetString() : null;
            if (!string.IsNullOrWhiteSpace(role))
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }
        catch (JsonException)
        {
            // A token whose metadata cannot be read is left as it came: the
            // request will fail later, when the role is required.
        }

        return Task.FromResult(principal);
    }

    private static void AddClaim(ClaimsIdentity identity, JsonElement root, string property, string claimType)
    {
        if (!root.TryGetProperty(property, out var value)) return;
        if (value.ValueKind != JsonValueKind.String) return;

        var content = value.GetString();
        if (string.IsNullOrWhiteSpace(content)) return;

        identity.AddClaim(new Claim(claimType, content));
    }
}
