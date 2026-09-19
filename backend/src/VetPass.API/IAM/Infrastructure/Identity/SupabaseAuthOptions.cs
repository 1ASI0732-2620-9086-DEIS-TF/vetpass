namespace VetPass.API.IAM.Infrastructure.Identity;

/// <summary>
/// Configuration of the identity provider. None of these values lives in the
/// repository: in development they are loaded with dotnet user-secrets.
/// </summary>
public class SupabaseAuthOptions
{
    public const string SectionName = "Supabase";

    /// <summary>Base URL of the project, https://&lt;ref&gt;.supabase.co.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Publishable key, used for the operations of the end user.</summary>
    public string AnonKey { get; set; } = string.Empty;

    /// <summary>
    /// Secret key, used only to create accounts from the server. It never
    /// reaches the web or the mobile application.
    /// </summary>
    public string ServiceRoleKey { get; set; } = string.Empty;

    public string AuthUrl => $"{Url.TrimEnd('/')}/auth/v1";

    public string JwksUrl => $"{AuthUrl}/.well-known/jwks.json";
}
