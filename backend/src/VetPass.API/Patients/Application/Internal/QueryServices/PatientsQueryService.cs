using VetPass.API.Patients.Domain.Model.Aggregates;
using VetPass.API.Patients.Domain.Model.ValueObjects;
using VetPass.API.Patients.Domain.Repositories;
using VetPass.API.Shared.Domain.Exceptions;

namespace VetPass.API.Patients.Application.Internal.QueryServices;

public class PatientsQueryService(IClientRepository clients, IPetRepository pets)
{
    public async Task<Patient> GetPatientAsync(Guid petId, CancellationToken cancellationToken = default) =>
        await pets.FindPatientByIdAsync(petId, cancellationToken)
        ?? throw new ResourceNotFoundException("una mascota", petId);

    public async Task<IReadOnlyList<Patient>> SearchAsync(Guid clinicId, string? term, Species? species,
        CancellationToken cancellationToken = default) =>
        await pets.SearchAsync(clinicId, term, species, cancellationToken);

    public async Task<IReadOnlyList<Patient>> ListByClientAsync(Guid clientId,
        CancellationToken cancellationToken = default) =>
        await pets.ListByClientAsync(clientId, cancellationToken);

    public async Task<IReadOnlyList<Client>> ListClientsAsync(Guid clinicId,
        CancellationToken cancellationToken = default) =>
        await clients.ListByClinicAsync(clinicId, cancellationToken);

    public async Task<Client> GetClientAsync(Guid clientId, CancellationToken cancellationToken = default) =>
        await clients.FindByIdAsync(clientId, cancellationToken)
        ?? throw new ResourceNotFoundException("un cliente", clientId);
}
