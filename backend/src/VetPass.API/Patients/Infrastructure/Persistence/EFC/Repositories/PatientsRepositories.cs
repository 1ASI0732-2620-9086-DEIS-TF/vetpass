using Microsoft.EntityFrameworkCore;
using VetPass.API.Patients.Domain.Model.Aggregates;
using VetPass.API.Patients.Domain.Model.ValueObjects;
using VetPass.API.Patients.Domain.Repositories;
using VetPass.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using VetPass.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace VetPass.API.Patients.Infrastructure.Persistence.EFC.Repositories;

public class ClientRepository(VetPassDbContext context)
    : BaseRepository<Client>(context), IClientRepository
{
    public async Task<Client?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Context.Clients.FirstOrDefaultAsync(client => client.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Client>> ListByClinicAsync(Guid clinicId,
        CancellationToken cancellationToken = default) =>
        await Context.Clients
            .Where(client => client.ClinicId == clinicId)
            .OrderBy(client => client.FullName)
            .ToListAsync(cancellationToken);
}

public class PetRepository(VetPassDbContext context)
    : BaseRepository<Pet>(context), IPetRepository
{
    public async Task<Pet?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Context.Pets.FirstOrDefaultAsync(pet => pet.Id == id, cancellationToken);

    public async Task<Patient?> FindPatientByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await JoinedPatients().FirstOrDefaultAsync(patient => patient.Pet.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Patient>> SearchAsync(Guid clinicId, string? term, Species? species,
        CancellationToken cancellationToken = default)
    {
        var query = JoinedPatients().Where(patient => patient.Owner.ClinicId == clinicId);

        if (!string.IsNullOrWhiteSpace(term))
        {
            var pattern = $"%{term.Trim()}%";
            query = query.Where(patient =>
                EF.Functions.ILike(patient.Pet.Name, pattern) ||
                EF.Functions.ILike(patient.Owner.FullName, pattern));
        }

        if (species is not null)
            query = query.Where(patient => patient.Pet.Species == species);

        // Alphabetical, so that a name can be located inside a long listing
        // without going through all of it (section 4.2.1).
        return await query.OrderBy(patient => patient.Pet.Name).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Patient>> ListByClientAsync(Guid clientId,
        CancellationToken cancellationToken = default) =>
        await JoinedPatients()
            .Where(patient => patient.Pet.ClientId == clientId)
            .OrderBy(patient => patient.Pet.Name)
            .ToListAsync(cancellationToken);

    private IQueryable<Patient> JoinedPatients() =>
        from pet in Context.Pets
        join client in Context.Clients on pet.ClientId equals client.Id
        select new Patient(pet, client);
}
