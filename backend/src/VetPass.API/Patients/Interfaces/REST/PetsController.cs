using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VetPass.API.Patients.Application.Internal.CommandServices;
using VetPass.API.Patients.Application.Internal.QueryServices;
using VetPass.API.Patients.Domain.Model.Commands;
using VetPass.API.Patients.Domain.Model.ValueObjects;
using VetPass.API.Patients.Interfaces.REST.Resources;
using VetPass.API.Patients.Interfaces.REST.Transform;
using VetPass.API.Shared.Domain.Exceptions;
using VetPass.API.Shared.Domain.Services;
using VetPass.API.Shared.Interfaces.ASP.Security;
using VetPass.API.Vaccination.Application.Internal.QueryServices;
using VetPass.API.Vaccination.Domain.Model.ValueObjects;

namespace VetPass.API.Patients.Interfaces.REST;

/// <summary>Endpoints of pets registered as patients (TS02).</summary>
[ApiController]
[Route("api/v1/pets")]
[Produces(MediaTypeNames.Application.Json)]
[Authorize]
[SwaggerTag("Patients — mascotas registradas como pacientes")]
public class PetsController(
    PatientsCommandService commandService,
    PatientsQueryService queryService,
    VaccinationQueryService vaccinationQueryService,
    IClinicClock clock,
    ICurrentUser currentUser) : ControllerBase
{
    /// <summary>
    /// Registers a pet and, with it, its vaccination card (US07, US09).
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ClinicStaff)]
    [ProducesResponseType(typeof(PetResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreatePetResource resource,
        CancellationToken cancellationToken)
    {
        var pet = await commandService.CreatePetAsync(
            new CreatePetCommand(resource.ClientId, resource.Name, resource.Species.ToSpecies(),
                resource.Breed, resource.Sex.ToSex(), resource.BirthDate),
            cancellationToken);

        var patient = await queryService.GetPatientAsync(pet.Id, cancellationToken);
        var created = PetResourceFromEntityAssembler.ToResource(patient, clock.Today);

        return CreatedAtAction(nameof(GetById), new { id = pet.Id }, created);
    }

    /// <summary>
    /// Locates patients by the name of the pet or of its owner, and narrows the
    /// listing by species and by status of the card (US08, section 4.2.4).
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.ClinicStaff)]
    [ProducesResponseType(typeof(IEnumerable<PatientResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string? search, [FromQuery] string? species,
        [FromQuery] string? cardStatus, CancellationToken cancellationToken)
    {
        var clinicId = currentUser.ClinicId
                       ?? throw new ForbiddenOperationException("El usuario no está asociado a ninguna clínica.");

        Species? speciesFilter = string.IsNullOrWhiteSpace(species) || species.Equals("all", StringComparison.OrdinalIgnoreCase)
            ? null
            : species.ToSpecies();

        var patients = await queryService.SearchAsync(clinicId, search, speciesFilter, cancellationToken);

        var statuses = await vaccinationQueryService.GetStatusesAsync(
            patients.Select(patient => patient.Pet.Id), cancellationToken);

        var today = clock.Today;
        var rows = patients.Select(patient => PatientResourceFromEntityAssembler.ToResource(
            patient, statuses.TryGetValue(patient.Pet.Id, out var status) ? status : null, today));

        if (!string.IsNullOrWhiteSpace(cardStatus) &&
            !cardStatus.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            if (!Enum.TryParse<CardStatus>(cardStatus, ignoreCase: true, out var wanted))
                throw new UnknownCardStatusException(cardStatus);

            rows = rows.Where(row => row.CardStatus == wanted.ToString());
        }

        return Ok(rows);
    }

    /// <summary>
    /// Record of a patient. The staff of the clinic reaches any of them; the
    /// owner, only those of their own client (US05-E2).
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PetResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var patient = await queryService.GetPatientAsync(id, cancellationToken);
        currentUser.EnsureCanReadClient(patient.Owner.Id);

        return Ok(PetResourceFromEntityAssembler.ToResource(patient, clock.Today));
    }
}

public class UnknownCardStatusException(string value)
    : InvalidDomainDataException(
        $"El estado de cartilla '{value}' no existe. Los valores admitidos son UpToDate, Pending y Overdue.")
{
    public override string Code => "unknown-card-status";
}
