using VetPass.API.Patients.Domain.Model.ValueObjects;
using VetPass.API.Shared.Infrastructure.Persistence.EFC.Seeding;
using VetPass.API.Vaccination.Domain.Model.ValueObjects;
using VetPass.API.Vaccination.Domain.Services;

namespace VetPass.UnitTests.Vaccination;

/// <summary>Canine schedule of SP01, the same template the catalog seeds.</summary>
internal static class CanineSchedule
{
    public static readonly Guid Multivalent = VaccinationCatalogSeeder.CanineMultivalentId;
    public static readonly Guid Rabies = VaccinationCatalogSeeder.CanineRabiesId;

    public static VaccinationSchedule Create() => new(Species.Canine,
    [
        new ScheduleItem(Guid.NewGuid(), Multivalent, Species.Canine, 1, 6, 0),
        new ScheduleItem(Guid.NewGuid(), Multivalent, Species.Canine, 2, 9, 3),
        new ScheduleItem(Guid.NewGuid(), Multivalent, Species.Canine, 3, 12, 3),
        new ScheduleItem(Guid.NewGuid(), Rabies, Species.Canine, 1, 12, 0),
        new ScheduleItem(Guid.NewGuid(), Multivalent, Species.Canine, 4, 52, 40),
        new ScheduleItem(Guid.NewGuid(), Rabies, Species.Canine, 2, 64, 52),
    ]);
}
