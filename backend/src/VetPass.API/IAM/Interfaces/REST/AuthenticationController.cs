using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VetPass.API.IAM.Application.Internal.CommandServices;
using VetPass.API.IAM.Application.Internal.QueryServices;
using VetPass.API.IAM.Domain.Model.Commands;
using VetPass.API.IAM.Domain.Model.ValueObjects;
using VetPass.API.IAM.Interfaces.REST.Resources;
using VetPass.API.IAM.Interfaces.REST.Transform;
using VetPass.API.Shared.Domain.Exceptions;
using VetPass.API.Shared.Interfaces.ASP.Security;

namespace VetPass.API.IAM.Interfaces.REST;

/// <summary>
/// Endpoints of authentication and of provisioning of accounts (TS01).
///
/// The applications never talk to the identity provider: they send their
/// credentials here, and this controller answers with the token and the role of
/// the user, so that the contract they consume is the one of this API alone.
/// </summary>
[ApiController]
[Route("api/v1/authentication")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Identity and Access — autenticación y alta de cuentas")]
public class AuthenticationController(
    AuthenticationCommandService commandService,
    UserProfileQueryService queryService,
    ICurrentUser currentUser) : ControllerBase
{
    /// <summary>Opens a session with e-mail and password (TS01-E1, TS01-E2).</summary>
    [HttpPost("sign-in")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticatedSessionResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SignIn([FromBody] SignInResource resource,
        CancellationToken cancellationToken)
    {
        var session = await commandService.SignInAsync(
            new SignInCommand(resource.Email, resource.Password), cancellationToken);

        return Ok(AuthenticatedSessionResourceFromEntityAssembler.ToResource(session));
    }

    /// <summary>Renews the session without asking for the credentials again.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshSessionResource resource,
        CancellationToken cancellationToken)
    {
        var token = await commandService.RefreshAsync(
            new RefreshSessionCommand(resource.RefreshToken), cancellationToken);

        return Ok(new
        {
            accessToken = token.Value,
            refreshToken = token.RefreshToken,
            expiresIn = token.ExpiresInSeconds,
            tokenType = "Bearer"
        });
    }

    /// <summary>Profile of the user holding the session.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResource), StatusCodes.Status200OK)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var vista = await queryService.GetViewByIdAsync(currentUser.Id, cancellationToken);
        return Ok(UserResourceFromEntityAssembler.ToResource(vista.Profile, vista.ClinicName));
    }

    /// <summary>
    /// Enrols a member of the staff in the clinic of the user who requests it.
    /// </summary>
    [HttpPost("staff-accounts")]
    [Authorize(Policy = AuthorizationPolicies.ClinicStaff)]
    [ProducesResponseType(typeof(CreatedAccountResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateStaffAccount([FromBody] CreateStaffAccountResource resource,
        CancellationToken cancellationToken)
    {
        var clinicId = currentUser.ClinicId
                       ?? throw new ForbiddenOperationException("El usuario no está asociado a ninguna clínica.");

        var account = await commandService.CreateAccountAsync(
            new CreateAccountCommand(resource.Email, resource.FullName, Role.ClinicStaff, clinicId, null),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created,
            CreatedAccountResourceFromEntityAssembler.ToResource(account));
    }

    /// <summary>
    /// Creates the access of the owner of a pet, whose credentials the clinic
    /// hands over at the reception desk (US05).
    /// </summary>
    [HttpPost("owner-accounts")]
    [Authorize(Policy = AuthorizationPolicies.ClinicStaff)]
    [ProducesResponseType(typeof(CreatedAccountResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateOwnerAccount([FromBody] CreateOwnerAccountResource resource,
        CancellationToken cancellationToken)
    {
        var clinicId = currentUser.ClinicId
                       ?? throw new ForbiddenOperationException("El usuario no está asociado a ninguna clínica.");

        var account = await commandService.CreateAccountAsync(
            new CreateAccountCommand(resource.Email, resource.FullName, Role.PetOwner, clinicId, resource.ClientId),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created,
            CreatedAccountResourceFromEntityAssembler.ToResource(account));
    }

    /// <summary>
    /// Replaces the password of the user holding the session with one of their
    /// own (US17). Answers 400, not 401, when the current password does not
    /// match, so that the applications do not take it for an expired session.
    /// </summary>
    [HttpPost("password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordResource resource,
        CancellationToken cancellationToken)
    {
        await commandService.ChangePasswordAsync(currentUser.Id, resource.CurrentPassword,
            resource.NewPassword, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Issues a new temporary password for the owner of a pet who forgot theirs
    /// (US18). It travels only in this answer, as when the account was created.
    /// </summary>
    [HttpPost("owner-accounts/{clientId:guid}/password-reset")]
    [Authorize(Policy = AuthorizationPolicies.ClinicStaff)]
    [ProducesResponseType(typeof(CreatedAccountResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetOwnerPassword(Guid clientId, CancellationToken cancellationToken)
    {
        var clinicId = currentUser.ClinicId
                       ?? throw new ForbiddenOperationException("El usuario no está asociado a ninguna clínica.");

        var account = await commandService.ResetOwnerPasswordAsync(clientId, clinicId, cancellationToken);
        return Ok(CreatedAccountResourceFromEntityAssembler.ToResource(account));
    }
}
