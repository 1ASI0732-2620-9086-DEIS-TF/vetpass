using System.Security.Claims;
using VetPass.API.IAM.Domain.Model.ValueObjects;
using VetPass.API.Shared.Domain.Exceptions;

namespace VetPass.API.Shared.Interfaces.ASP.Security;

/// <summary>
/// The user behind the request, read from the claims of the token that Supabase
/// Auth issued and the API validated.
/// </summary>
public interface ICurrentUser
{
    Guid Id { get; }
    Role Role { get; }
    Guid? ClinicId { get; }
    Guid? ClientId { get; }

    /// <summary>
    /// Guards the information of a client against a request from another one:
    /// the staff of the clinic reaches every patient, and the owner reaches only
    /// those of the client the account belongs to (US05-E2).
    /// </summary>
    void EnsureCanReadClient(Guid clientId);
}

public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public const string RoleClaim = "vetpass_role";
    public const string ClinicClaim = "vetpass_clinic_id";
    public const string ClientClaim = "vetpass_client_id";

    private ClaimsPrincipal Principal => accessor.HttpContext?.User
        ?? throw new ForbiddenOperationException("La solicitud no presenta un usuario autenticado.");

    public Guid Id => Guid.TryParse(Principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id
        : throw new ForbiddenOperationException("El token no identifica a ningún usuario.");

    public Role Role => Principal.FindFirstValue(RoleClaim).ToRole();

    public Guid? ClinicId => Guid.TryParse(Principal.FindFirstValue(ClinicClaim), out var id) ? id : null;

    public Guid? ClientId => Guid.TryParse(Principal.FindFirstValue(ClientClaim), out var id) ? id : null;

    public void EnsureCanReadClient(Guid clientId)
    {
        if (Role == Role.ClinicStaff) return;
        if (ClientId == clientId) return;

        throw new ForbiddenOperationException(
            "La información solicitada pertenece a otro cliente de la clínica.");
    }
}
