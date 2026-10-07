using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VetPass.API.IAM.Domain.Repositories;
using VetPass.API.Patients.Application.Internal.CommandServices;
using VetPass.API.Patients.Application.Internal.QueryServices;
using VetPass.API.Patients.Domain.Model.Commands;
using VetPass.API.Patients.Domain.Model.ValueObjects;
using VetPass.API.Patients.Interfaces.REST.Resources;
using VetPass.API.Patients.Interfaces.REST.Transform;
using VetPass.API.Shared.Domain.Exceptions;
using VetPass.API.Shared.Interfaces.ASP.Security;

namespace VetPass.API.Patients.Interfaces.REST;

/// <summary>Endpoints of clients (TS02).</summary>
[ApiController]
[Route("api/v1/clients")]
[Produces(MediaTypeNames.Application.Json)]
[Authorize(Policy = AuthorizationPolicies.ClinicStaff)]
[SwaggerTag("Patients — clientes de la clínica")]
public class ClientsController(
    PatientsCommandService commandService,
    PatientsQueryService queryService,
    IUserProfileRepository userProfiles,
    ICurrentUser currentUser) : ControllerBase
{
    /// <summary>Registers a client of the clinic (US06).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClientResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateClientResource resource,
        CancellationToken cancellationToken)
    {
        var client = await commandService.CreateClientAsync(
            new CreateClientCommand(ClinicOfCurrentUser(), resource.FullName,
                PhoneNumber.Parse(resource.PhoneNumber), resource.Email),
            cancellationToken);

        var created = ClientResourceFromEntityAssembler.ToResource(client);
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, created);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClientResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var clinicId = ClinicOfCurrentUser();
        var clients = await queryService.ListClientsAsync(clinicId, cancellationToken);
        var conAcceso = await userProfiles.ListClientIdsWithAccountAsync(clinicId, cancellationToken);

        return Ok(clients.Select(client =>
            ClientResourceFromEntityAssembler.ToResource(client, conAcceso.Contains(client.Id))));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClientResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var client = await queryService.GetClientAsync(id, cancellationToken);
        return Ok(ClientResourceFromEntityAssembler.ToResource(client));
    }

    private Guid ClinicOfCurrentUser() => currentUser.ClinicId
        ?? throw new ForbiddenOperationException("El usuario no está asociado a ninguna clínica.");
}
