using Microsoft.EntityFrameworkCore;
using VetPass.API.Patients.Domain.Model.ValueObjects;
using VetPass.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using VetPass.API.Vaccination.Domain.Model.Entities;
using VetPass.API.Vaccination.Domain.Model.ValueObjects;

namespace VetPass.API.Shared.Infrastructure.Persistence.EFC.Seeding;

/// <summary>
/// Loads the catalogue of vaccines and the template of the schedule, whose
/// values come from the SP01 spike documented in docs/spikes.
///
/// The identifiers are fixed, so that running the seed again neither duplicates
/// rows nor breaks the cards already issued that point at them. Correcting a
/// minimum age after the validation of a veterinarian is a change in this file,
/// not in the domain.
/// </summary>
public static class VaccinationCatalogSeeder
{
    private static Guid Vaccine(int n) => Guid.Parse($"11111111-1111-4111-8111-{n:D12}");
    private static Guid Item(int n) => Guid.Parse($"22222222-2222-4222-8222-{n:D12}");

    // Públicos porque el seed de demostración necesita nombrar estas vacunas
    // para registrar las dosis ya aplicadas de cada mascota.
    public static readonly Guid CanineMultivalentId = Vaccine(1);
    public static readonly Guid CanineRabiesId = Vaccine(2);
    public static readonly Guid FelineMultivalentId = Vaccine(3);
    public static readonly Guid FelineLeukemiaId = Vaccine(4);
    public static readonly Guid FelineRabiesId = Vaccine(5);

    public static async Task SeedAsync(VetPassDbContext context, CancellationToken cancellationToken = default)
    {
        await SeedVaccinesAsync(context, cancellationToken);
        await SeedScheduleAsync(context, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedVaccinesAsync(VetPassDbContext context, CancellationToken cancellationToken)
    {
        var existing = await context.Vaccines.Select(vaccine => vaccine.Id).ToListAsync(cancellationToken);

        var vaccines = new[]
        {
            new Vaccine(CanineMultivalentId, "Quíntuple", Species.Canine, true),
            new Vaccine(CanineRabiesId, "Antirrábica", Species.Canine, true),
            new Vaccine(FelineMultivalentId, "Triple felina", Species.Feline, true),
            new Vaccine(FelineLeukemiaId, "Leucemia felina", Species.Feline, false),
            new Vaccine(FelineRabiesId, "Antirrábica", Species.Feline, true)
        };

        foreach (var vaccine in vaccines.Where(vaccine => !existing.Contains(vaccine.Id)))
            await context.Vaccines.AddAsync(vaccine, cancellationToken);
    }

    private static async Task SeedScheduleAsync(VetPassDbContext context, CancellationToken cancellationToken)
    {
        var existing = await context.ScheduleItems.Select(item => item.Id).ToListAsync(cancellationToken);

        var items = new[]
        {
            // Esquema canino (sección 4 de SP01)
            new ScheduleItem(Item(1), CanineMultivalentId, Species.Canine, 1, 6, 0),
            new ScheduleItem(Item(2), CanineMultivalentId, Species.Canine, 2, 9, 3),
            new ScheduleItem(Item(3), CanineMultivalentId, Species.Canine, 3, 12, 3),
            new ScheduleItem(Item(4), CanineRabiesId, Species.Canine, 1, 12, 0),
            new ScheduleItem(Item(5), CanineMultivalentId, Species.Canine, 4, 52, 40),
            new ScheduleItem(Item(6), CanineRabiesId, Species.Canine, 2, 64, 52),

            // Esquema felino (sección 5 de SP01)
            new ScheduleItem(Item(7), FelineMultivalentId, Species.Feline, 1, 6, 0),
            new ScheduleItem(Item(8), FelineMultivalentId, Species.Feline, 2, 9, 3),
            new ScheduleItem(Item(9), FelineMultivalentId, Species.Feline, 3, 12, 3),
            new ScheduleItem(Item(10), FelineLeukemiaId, Species.Feline, 1, 8, 0),
            new ScheduleItem(Item(11), FelineLeukemiaId, Species.Feline, 2, 11, 3),
            new ScheduleItem(Item(12), FelineRabiesId, Species.Feline, 1, 12, 0),
            new ScheduleItem(Item(13), FelineMultivalentId, Species.Feline, 4, 52, 40),
            new ScheduleItem(Item(14), FelineLeukemiaId, Species.Feline, 3, 60, 49),
            new ScheduleItem(Item(15), FelineRabiesId, Species.Feline, 2, 64, 52)
        };

        foreach (var item in items.Where(item => !existing.Contains(item.Id)))
            await context.ScheduleItems.AddAsync(item, cancellationToken);
    }
}
