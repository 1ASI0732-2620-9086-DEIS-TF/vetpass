using Microsoft.EntityFrameworkCore;
using VetPass.API.Patients.Domain.Model.Aggregates;
using VetPass.API.Patients.Domain.Model.ValueObjects;
using VetPass.API.Patients.Domain.Repositories;
using VetPass.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using VetPass.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace VetPass.API.Patients.Infrastructure.Persistence.EFC.Repositories;

public partial class ClientRepository(VetPassDbContext context)
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

public partial class ClientRepository
{
    public async Task<Client?> FindByDocumentAsync(Guid clinicId, IdentityDocument document,
        CancellationToken cancellationToken = default) =>
        await Context.Clients.FirstOrDefaultAsync(client =>
            client.ClinicId == clinicId &&
            client.DocumentType == document.Type &&
            client.DocumentNumber == document.Number, cancellationToken);
}

public class PetRepository(VetPassDbContext context)
    : BaseRepository<Pet>(context), IPetRepository
{
    public async Task<Pet?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Context.Pets.FirstOrDefaultAsync(pet => pet.Id == id, cancellationToken);

    public async Task<Patient?> FindPatientByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await (from pet in Context.Pets
               join client in Context.Clients on pet.ClientId equals client.Id
               where pet.Id == id
               select new Patient(pet, client))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<Patient>> SearchAsync(Guid clinicId, string? term, Species? species,
        CancellationToken cancellationToken = default)
    {
        // El filtrado ocurre sobre el par mascota-cliente y la proyección al
        // resultado se deja para el final: un filtro aplicado sobre el objeto
        // ya proyectado no es traducible a SQL.
        var query = from pet in Context.Pets
                    join client in Context.Clients on pet.ClientId equals client.Id
                    where client.ClinicId == clinicId
                    select new { pet, client };

        if (!string.IsNullOrWhiteSpace(term))
        {
            var pattern = $"%{term.Trim()}%";
            query = query.Where(row =>
                EF.Functions.ILike(row.pet.Name, pattern) ||
                EF.Functions.ILike(row.client.FullName, pattern));
        }

        if (species is not null)
            query = query.Where(row => row.pet.Species == species);

        // Alfabético, para ubicar un nombre dentro de un listado extenso sin
        // recorrerlo por completo (sección 4.2.1).
        return await query
            .OrderBy(row => row.pet.Name)
            .Select(row => new Patient(row.pet, row.client))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Patient>> ListByClientAsync(Guid clientId,
        CancellationToken cancellationToken = default) =>
        await (from pet in Context.Pets
               join client in Context.Clients on pet.ClientId equals client.Id
               where pet.ClientId == clientId
               orderby pet.Name
               select new Patient(pet, client))
            .ToListAsync(cancellationToken);
}
