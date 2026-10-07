using VetPass.API.Patients.Domain.Model.Aggregates;
using VetPass.API.Patients.Domain.Model.ValueObjects;

namespace VetPass.API.Patients.Domain.Repositories;

/// <summary>A pet together with the client that owns it, which is how the
/// listing of patients presents it (section 4.2.4).</summary>
public record Patient(Pet Pet, Client Owner);

public interface IClientRepository
{
    Task<Client?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Client>> ListByClinicAsync(Guid clinicId, CancellationToken cancellationToken = default);

    /// <summary>The client of the clinic that holds the document, if any.</summary>
    Task<Client?> FindByDocumentAsync(Guid clinicId, IdentityDocument document,
        CancellationToken cancellationToken = default);
    Task AddAsync(Client client, CancellationToken cancellationToken = default);
}

public interface IPetRepository
{
    Task<Pet?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Patient?> FindPatientByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Locates patients by the name of the pet or by the name of its owner,
    /// without asking the user to choose the criterion beforehand (US08).
    /// </summary>
    Task<IReadOnlyList<Patient>> SearchAsync(Guid clinicId, string? term, Species? species,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Patient>> ListByClientAsync(Guid clientId, CancellationToken cancellationToken = default);

    Task AddAsync(Pet pet, CancellationToken cancellationToken = default);
}
