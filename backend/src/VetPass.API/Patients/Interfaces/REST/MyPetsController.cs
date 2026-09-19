using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VetPass.API.Patients.Application.Internal.QueryServices;
using VetPass.API.Patients.Interfaces.REST.Resources;
using VetPass.API.Patients.Interfaces.REST.Transform;
using VetPass.API.Shared.Domain.Exceptions;
using VetPass.API.Shared.Domain.Services;
using VetPass.API.Shared.Interfaces.ASP.Security;
using VetPass.API.Vaccination.Application.Internal.QueryServices;

namespace VetPass.API.Patients.Interfaces.REST;

/// <summary>
/// Entry point of the mobile application: the pets of the client holding the
/// session, each one with the status of its own card (US12-E2).
/// </summary>
[ApiController]
[Route("api/v1/me/pets")]
[Produces(MediaTypeNames.Application.Json)]
[Authorize]
[SwaggerTag("Patients — mis mascotas (aplicación móvil)")]
public class MyPetsController(
    PatientsQueryService queryService,
    VaccinationQueryService vaccinationQueryService,
    IClinicClock clock,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PatientResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var clientId = currentUser.ClientId
                       ?? throw new ForbiddenOperationException(
                           "La cuenta en sesión no corresponde al dueño de una mascota.");

        var patients = await queryService.ListByClientAsync(clientId, cancellationToken);
        var statuses = await vaccinationQueryService.GetStatusesAsync(
            patients.Select(patient => patient.Pet.Id), cancellationToken);

        var today = clock.Today;
        return Ok(patients.Select(patient => PatientResourceFromEntityAssembler.ToResource(
            patient, statuses.TryGetValue(patient.Pet.Id, out var status) ? status : null, today)));
    }
}
