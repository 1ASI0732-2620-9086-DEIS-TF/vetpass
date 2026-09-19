using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VetPass.API.MedicalRecords.Application.Internal.CommandServices;
using VetPass.API.MedicalRecords.Application.Internal.QueryServices;
using VetPass.API.MedicalRecords.Domain.Model.Commands;
using VetPass.API.MedicalRecords.Interfaces.REST.Resources;
using VetPass.API.MedicalRecords.Interfaces.REST.Transform;
using VetPass.API.Patients.Application.Internal.QueryServices;
using VetPass.API.Shared.Interfaces.ASP.Security;

namespace VetPass.API.MedicalRecords.Interfaces.REST;

/// <summary>Endpoints of the veterinary record and its prescriptions (TS04).</summary>
[ApiController]
[Route("api/v1")]
[Produces(MediaTypeNames.Application.Json)]
[Authorize]
[SwaggerTag("Medical Records — atenciones veterinarias y recetas")]
public class VisitsController(
    MedicalRecordsCommandService commandService,
    MedicalRecordsQueryService queryService,
    PatientsQueryService patientsQueryService,
    ICurrentUser currentUser) : ControllerBase
{
    /// <summary>History of a patient, most recent first (US14, US16).</summary>
    [HttpGet("pets/{petId:guid}/visits")]
    [ProducesResponseType(typeof(IEnumerable<VisitResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListByPet(Guid petId, CancellationToken cancellationToken)
    {
        await EnsureAccessToPetAsync(petId, cancellationToken);

        var visits = await queryService.ListByPetIdAsync(petId, cancellationToken);
        return Ok(visits.Select(VisitResourceFromEntityAssembler.ToResource));
    }

    /// <summary>
    /// Registers a visit and, when it comes with medications, its prescription
    /// (US13, US15).
    /// </summary>
    [HttpPost("pets/{petId:guid}/visits")]
    [Authorize(Policy = AuthorizationPolicies.ClinicStaff)]
    [ProducesResponseType(typeof(VisitResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Register(Guid petId, [FromBody] RegisterVisitResource resource,
        CancellationToken cancellationToken)
    {
        // The pet must exist before writing anything in its history.
        _ = await patientsQueryService.GetPatientAsync(petId, cancellationToken);

        var visit = await commandService.RegisterVisitAsync(
            new RegisterVisitCommand(petId, resource.VeterinarianId ?? currentUser.Id, resource.VisitDate,
                resource.Reason, resource.Findings, resource.Diagnosis, resource.Treatment, resource.WeightKg),
            cancellationToken);

        if (resource.Prescription is { Count: > 0 })
        {
            visit = await commandService.IssuePrescriptionAsync(
                new IssuePrescriptionCommand(visit.Id, resource.Prescription
                    .Select(item => new PrescriptionItemCommand(item.Medication, item.Dosage, item.Duration))
                    .ToList()),
                cancellationToken);
        }

        var created = VisitResourceFromEntityAssembler.ToResource(visit);
        return CreatedAtAction(nameof(GetById), new { id = visit.Id }, created);
    }

    /// <summary>Detail of a visit with its prescription (US16-E2).</summary>
    [HttpGet("visits/{id:guid}")]
    [ProducesResponseType(typeof(VisitResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var visit = await queryService.GetByIdAsync(id, cancellationToken);
        await EnsureAccessToPetAsync(visit.PetId, cancellationToken);

        return Ok(VisitResourceFromEntityAssembler.ToResource(visit));
    }

    /// <summary>Issues the prescription of a visit already registered (US15).</summary>
    [HttpPost("visits/{id:guid}/prescription")]
    [Authorize(Policy = AuthorizationPolicies.ClinicStaff)]
    [ProducesResponseType(typeof(VisitResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IssuePrescription(Guid id,
        [FromBody] IssuePrescriptionResource resource, CancellationToken cancellationToken)
    {
        var visit = await commandService.IssuePrescriptionAsync(
            new IssuePrescriptionCommand(id, resource.Items
                .Select(item => new PrescriptionItemCommand(item.Medication, item.Dosage, item.Duration))
                .ToList()),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, VisitResourceFromEntityAssembler.ToResource(visit));
    }

    private async Task EnsureAccessToPetAsync(Guid petId, CancellationToken cancellationToken)
    {
        var patient = await patientsQueryService.GetPatientAsync(petId, cancellationToken);
        currentUser.EnsureCanReadClient(patient.Owner.Id);
    }
}
