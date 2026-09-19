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
        var profile = await queryService.GetByIdAsync(currentUser.Id, cancellationToken);
        return Ok(UserResourceFromEntityAssembler.ToResource(profile));
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
}
