using VetPass.API.Patients.Domain.Model.Aggregates;
using VetPass.API.Patients.Domain.Model.Commands;
using VetPass.API.Patients.Domain.Model.ValueObjects;
using VetPass.API.Patients.Domain.Repositories;
using VetPass.API.Shared.Domain.Exceptions;
using VetPass.API.Shared.Domain.Services;
using VetPass.API.Vaccination.Application.Internal.CommandServices;
using VetPass.API.Vaccination.Domain.Model.Commands;

namespace VetPass.API.Patients.Application.Internal.CommandServices;

public class PatientsCommandService(
    IClientRepository clients,
    IPetRepository pets,
    VaccinationCommandService vaccination,
    IClinicClock clock,
    IUnitOfWork unitOfWork)
{
    public async Task<Client> CreateClientAsync(CreateClientCommand command,
        CancellationToken cancellationToken = default)
    {
        // La misma persona no se registra dos veces: si su documento ya está en
        // la clínica, la respuesta nombra al cliente existente para que la
        // interfaz ofrezca usarlo (US06-E4).
        var existing = await clients.FindByDocumentAsync(command.ClinicId, command.Document, cancellationToken);
        if (existing is not null)
            throw new DuplicateClientException(command.Document, existing.Id, existing.FullName);

        var client = new Client(command.ClinicId, command.FullName, command.Document,
            command.PhoneNumber, command.Email);

        await clients.AddAsync(client, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return client;
    }

    /// <summary>
    /// Registers a pet and generates its vaccination card in the same
    /// transaction. This is the point where the Patients context asks the
    /// Vaccination context for something, and the only one: it hands over the
    /// species and the birth date, and receives nothing back that it has to
    /// keep (US07, US09).
    /// </summary>
    public async Task<Pet> CreatePetAsync(CreatePetCommand command,
        CancellationToken cancellationToken = default)
    {
        _ = await clients.FindByIdAsync(command.ClientId, cancellationToken)
            ?? throw new ResourceNotFoundException("un cliente", command.ClientId);

        var pet = new Pet(command.ClientId, command.Name, command.Species, command.Breed,
            command.Sex, command.BirthDate, clock.Today);

        await pets.AddAsync(pet, cancellationToken);

        await vaccination.GenerateForAsync(
            new GenerateVaccinationCardCommand(pet.Id, pet.Species, pet.BirthDate), cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);

        return pet;
    }
}

public class DuplicateClientException(IdentityDocument document, Guid existingClientId, string existingClientName)
    : DomainConflictException(
        $"Ya existe un cliente con el documento {document.Number}: {existingClientName}.")
{
    public override string Code => "duplicate-client";
    public Guid ExistingClientId { get; } = existingClientId;
    public string ExistingClientName { get; } = existingClientName;
}
