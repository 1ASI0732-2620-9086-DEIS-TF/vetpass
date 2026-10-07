using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VetPass.API.Patients.Application.Internal.QueryServices;
using VetPass.API.Shared.Interfaces.ASP.Security;
using VetPass.API.Vaccination.Application.Internal.CommandServices;
using VetPass.API.Vaccination.Application.Internal.QueryServices;
using VetPass.API.Vaccination.Domain.Model.Commands;
using VetPass.API.Vaccination.Domain.Model.ValueObjects;
using VetPass.API.Vaccination.Interfaces.REST.Resources;
using VetPass.API.Vaccination.Interfaces.REST.Transform;

namespace VetPass.API.Vaccination.Interfaces.REST;

/// <summary>
/// Endpoints of the vaccination card (TS03). The card is consulted by both
/// applications and written only from the clinic.
/// </summary>
[ApiController]
[Route("api/v1/pets/{petId:guid}/vaccination-card")]
[Produces(MediaTypeNames.Application.Json)]
[Authorize]
[SwaggerTag("Vaccination — cartilla de vacunación y registro de dosis")]
public class VaccinationCardsController(
    VaccinationCommandService commandService,
    VaccinationQueryService queryService,
    PatientsQueryService patientsQueryService,
    ICurrentUser currentUser) : ControllerBase
{
    /// <summary>
    /// Card of a pet with its doses and the status of the schedule (US11, US12).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(VaccinationCardResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByPetId(Guid petId, CancellationToken cancellationToken)
    {
        await EnsureAccessToPetAsync(petId, cancellationToken);

        var view = await queryService.GetByPetIdAsync(petId, cancellationToken);
        return Ok(VaccinationCardResourceFromEntityAssembler.ToResource(view));
    }

    /// <summary>
    /// Records an applied dose (US10). Answers 201 when the schedule admits it,
    /// and 422 naming the rule that was not met when it does not.
    /// </summary>
    [HttpPost("doses/{doseId:guid}/application")]
    [Authorize(Policy = AuthorizationPolicies.ClinicStaff)]
    [ProducesResponseType(typeof(VaccinationCardResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegisterDose(Guid petId, Guid doseId,
        [FromBody] RegisterDoseResource resource, CancellationToken cancellationToken)
    {
        await EnsureAccessToPetAsync(petId, cancellationToken);

        await commandService.RegisterDoseAsync(
            new RegisterDoseCommand(petId, doseId, resource.ApplicationDate,
                new BatchCode(resource.BatchCode), resource.VeterinarianId ?? currentUser.Id),
            cancellationToken);

        var view = await queryService.GetByPetIdAsync(petId, cancellationToken);
        return StatusCode(StatusCodes.Status201Created,
            VaccinationCardResourceFromEntityAssembler.ToResource(view));
    }

    private async Task EnsureAccessToPetAsync(Guid petId, CancellationToken cancellationToken)
    {
        var patient = await patientsQueryService.GetPatientAsync(petId, cancellationToken);
        currentUser.EnsureCanAccess(patient.Owner.ClinicId, patient.Owner.Id);
    }
}
