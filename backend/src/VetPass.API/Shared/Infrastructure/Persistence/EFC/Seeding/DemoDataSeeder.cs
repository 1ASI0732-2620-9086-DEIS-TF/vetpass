using Microsoft.EntityFrameworkCore;
using VetPass.API.IAM.Domain.Model.Aggregates;
using VetPass.API.IAM.Domain.Model.ValueObjects;
using VetPass.API.IAM.Domain.Services;
using VetPass.API.MedicalRecords.Domain.Model.Aggregates;
using VetPass.API.MedicalRecords.Domain.Model.ValueObjects;
using VetPass.API.Patients.Domain.Model.Aggregates;
using VetPass.API.Patients.Domain.Model.ValueObjects;
using VetPass.API.Shared.Domain.Services;
using VetPass.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using VetPass.API.Vaccination.Domain.Model.Aggregates;
using VetPass.API.Vaccination.Domain.Model.ValueObjects;
using VetPass.API.Vaccination.Domain.Services;

namespace VetPass.API.Shared.Infrastructure.Persistence.EFC.Seeding;

/// <summary>
/// Loads the demonstration case of the mock-ups of sections 4.4 and 4.6:
/// Veterinaria San Miguel, its staff, its clients and their pets, with the
/// visits and prescriptions those screens show.
///
/// Every date is calculated relative to the day the seed runs, and not written
/// as a fixed date. That is what keeps the case alive: Rocky is always a
/// nine-week-old puppy whose second dose falls due today, so the rule of US10
/// remains visible in the demonstration months from now.
///
/// The clinical data are for demonstration and do not come from a documentary
/// source.
/// </summary>
public class DemoDataSeeder(
    VetPassDbContext context,
    IIdentityProvider identityProvider,
    IVaccinationScheduleProvider scheduleProvider,
    IClinicClock clock,
    ILogger<DemoDataSeeder> logger)
{
    private static readonly Guid ClinicId = Guid.Parse("33333333-3333-4333-8333-000000000001");

    private const string StaffEmail = "andrea.quispe@vetsanmiguel.pe";
    private const string OwnerEmail = "valeria.campos@correo.com";

    public async Task SeedAsync(string password, CancellationToken cancellationToken = default)
    {
        if (await context.Clinics.AnyAsync(clinic => clinic.Id == ClinicId, cancellationToken))
        {
            logger.LogInformation("El caso de demostración ya está cargado; no se vuelve a sembrar.");
            return;
        }

        var today = clock.Today;

        var clinic = new Clinic(ClinicId, "Veterinaria San Miguel", "Av. La Marina 2340, San Miguel");
        await context.Clinics.AddAsync(clinic, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var veterinarian = await CreateAccountAsync(StaffEmail, "Andrea Quispe", Role.ClinicStaff,
            ClinicId, null, password, cancellationToken);

        // Los clientes de la clínica. Valeria Campos es la User Persona de la
        // sección 2.3.1 y la dueña que aparece en los mock-ups de la aplicación
        // móvil, con sus dos mascotas.
        // Los documentos son ficticios. Carlos Ramos tiene carné de extranjería,
        // para que el caso muestre los dos tipos que la plataforma admite.
        var valeria = await AddClientAsync("Valeria Campos", Dni("45879123"), "987 654 321", OwnerEmail, cancellationToken);
        var jorge = await AddClientAsync("Jorge Aliaga", Dni("40112358"), "941 220 118", "jorge.aliaga@correo.com", cancellationToken);
        var melissa = await AddClientAsync("Melissa Rojas", Dni("47263519"), "915 883 204", "melissa.rojas@correo.com", cancellationToken);
        var carlos = await AddClientAsync("Carlos Ramos",
            IdentityDocument.Of(IdentityDocumentType.ForeignerCard, "001827364"), "998 471 663", null, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await CreateAccountAsync(OwnerEmail, "Valeria Campos", Role.PetOwner,
            ClinicId, valeria.Id, password, cancellationToken);

        // Rocky: el caso de la sección 4.6. Cachorro de nueve semanas con la
        // primera dosis aplicada, cuya segunda dosis corresponde hoy y cuya
        // antirrábica será rechazada por edad mínima.
        var rocky = await AddPetAsync(jorge.Id, "Rocky", Species.Canine, "Beagle", Sex.Male,
            today.AddDays(-9 * 7), cancellationToken);
        var rockyCard = await GenerateCardAsync(rocky, FirstVisit(rocky), cancellationToken);
        Apply(rockyCard, VaccinationCatalogSeeder.CanineMultivalentId, 1, today.AddDays(-3 * 7), "A-4471",
            veterinarian.Id);

        // Kiara: cartilla al día, con el refuerzo anual esperado más adelante.
        var kiara = await AddPetAsync(valeria.Id, "Kiara", Species.Canine, "Shih tzu", Sex.Female,
            today.AddDays(-50 * 7), cancellationToken);
        var kiaraCard = await GenerateCardAsync(kiara, FirstVisit(kiara), cancellationToken);
        ApplyInitialCanineSeries(kiaraCard, kiara.BirthDate, veterinarian.Id, "A-4471", "A-4520", "R-1180");

        // Simón: cartilla vencida por el refuerzo de leucemia felina.
        var simon = await AddPetAsync(valeria.Id, "Simón", Species.Feline, "Mestizo", Sex.Male,
            today.AddDays(-62 * 7), cancellationToken);
        var simonCard = await GenerateCardAsync(simon, FirstVisit(simon), cancellationToken);
        ApplyInitialFelineSeries(simonCard, simon.BirthDate, veterinarian.Id);
        Apply(simonCard, VaccinationCatalogSeeder.FelineMultivalentId, 4, simon.BirthDate.AddDays(52 * 7),
            "F-2214", veterinarian.Id);

        var luna = await AddPetAsync(jorge.Id, "Luna", Species.Canine, "Labrador", Sex.Female,
            today.AddDays(-50 * 7), cancellationToken);
        var lunaCard = await GenerateCardAsync(luna, FirstVisit(luna), cancellationToken);
        ApplyInitialCanineSeries(lunaCard, luna.BirthDate, veterinarian.Id, "A-4502", "A-4610", "R-1204");

        var michi = await AddPetAsync(melissa.Id, "Michi", Species.Feline, "Mestizo", Sex.Female,
            today.AddDays(-50 * 7), cancellationToken);
        var michiCard = await GenerateCardAsync(michi, FirstVisit(michi), cancellationToken);
        ApplyInitialFelineSeries(michiCard, michi.BirthDate, veterinarian.Id);

        // Toby: solo recibió la primera dosis, de modo que su cartilla está vencida.
        var toby = await AddPetAsync(carlos.Id, "Toby", Species.Canine, "Mestizo", Sex.Male,
            today.AddDays(-20 * 7), cancellationToken);
        var tobyCard = await GenerateCardAsync(toby, FirstVisit(toby), cancellationToken);
        Apply(tobyCard, VaccinationCatalogSeeder.CanineMultivalentId, 1, toby.BirthDate.AddDays(6 * 7),
            "A-4388", veterinarian.Id);

        await SeedVisitsAsync(rocky, kiara, veterinarian.Id, today, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Caso de demostración cargado. Personal: {Staff} · Dueña: {Owner} · Contraseña: {Password}",
            StaffEmail, OwnerEmail, password);
    }

    private async Task<UserProfile> CreateAccountAsync(string email, string fullName, Role role,
        Guid? clinicId, Guid? clientId, string password, CancellationToken cancellationToken)
    {
        var existing = await context.UserProfiles
            .FirstOrDefaultAsync(profile => profile.Email == email, cancellationToken);
        if (existing is not null) return existing;

        var account = await identityProvider.CreateAccountAsync(email, password, role,
            clinicId, clientId, cancellationToken);

        var profile = new UserProfile(account.Id, email, fullName, role, clinicId, clientId);
        await context.UserProfiles.AddAsync(profile, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return profile;
    }

    private static IdentityDocument Dni(string number) => IdentityDocument.Of(IdentityDocumentType.Dni, number);

    /// <summary>Day of the first visit, when the clinic registered the pet: its first dose.</summary>
    private static DateOnly FirstVisit(Pet pet) => pet.BirthDate.AddDays(6 * 7);

    private async Task<Client> AddClientAsync(string fullName, IdentityDocument document, string phone,
        string? email, CancellationToken cancellationToken)
    {
        var client = new Client(ClinicId, fullName, document, PhoneNumber.Parse(phone), email);
        await context.Clients.AddAsync(client, cancellationToken);
        return client;
    }

    private async Task<Pet> AddPetAsync(Guid clientId, string name, Species species, string breed,
        Sex sex, DateOnly birthDate, CancellationToken cancellationToken)
    {
        var pet = new Pet(clientId, name, species, breed, sex, birthDate, clock.Today);
        await context.Pets.AddAsync(pet, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return pet;
    }

    private async Task<VaccinationCard> GenerateCardAsync(Pet pet, DateOnly registeredOn,
        CancellationToken cancellationToken)
    {
        var schedule = await scheduleProvider.GetForAsync(pet.Species, cancellationToken);
        var card = VaccinationCard.GenerateFrom(pet.Id, schedule, pet.BirthDate, registeredOn);

        await context.VaccinationCards.AddAsync(card, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return card;
    }

    /// <summary>
    /// Records an applied dose through the aggregate, never by writing the row
    /// directly: the demonstration data are therefore data the domain itself
    /// admits, and seeding an impossible card is not even possible.
    ///
    /// The dose is recorded as the clinic recorded it, on the day it was
    /// applied. That is the "today" the aggregate plans the rest of the series
    /// from, so the history comes out as it happened and not compressed into
    /// the day the seed runs.
    /// </summary>
    private static void Apply(VaccinationCard card, Guid vaccineId, int sequenceNumber,
        DateOnly applicationDate, string batchCode, Guid veterinarianId)
    {
        var dose = card.Doses.First(d => d.VaccineId == vaccineId && d.SequenceNumber == sequenceNumber);
        card.RegisterDose(dose.Id, applicationDate, new BatchCode(batchCode), veterinarianId,
            today: applicationDate);
    }

    private static void ApplyInitialCanineSeries(VaccinationCard card, DateOnly birthDate,
        Guid veterinarianId, string firstBatch, string secondBatch, string rabiesBatch)
    {
        var multivalent = VaccinationCatalogSeeder.CanineMultivalentId;
        Apply(card, multivalent, 1, birthDate.AddDays(6 * 7), firstBatch, veterinarianId);
        Apply(card, multivalent, 2, birthDate.AddDays(9 * 7), secondBatch, veterinarianId);
        Apply(card, multivalent, 3, birthDate.AddDays(12 * 7), secondBatch, veterinarianId);
        Apply(card, VaccinationCatalogSeeder.CanineRabiesId, 1, birthDate.AddDays(12 * 7), rabiesBatch,
            veterinarianId);
    }

    private static void ApplyInitialFelineSeries(VaccinationCard card, DateOnly birthDate,
        Guid veterinarianId)
    {
        var multivalent = VaccinationCatalogSeeder.FelineMultivalentId;
        var leukemia = VaccinationCatalogSeeder.FelineLeukemiaId;

        Apply(card, multivalent, 1, birthDate.AddDays(6 * 7), "F-1120", veterinarianId);
        Apply(card, multivalent, 2, birthDate.AddDays(9 * 7), "F-1188", veterinarianId);
        Apply(card, multivalent, 3, birthDate.AddDays(12 * 7), "F-1240", veterinarianId);
        Apply(card, leukemia, 1, birthDate.AddDays(8 * 7), "L-0455", veterinarianId);
        Apply(card, leukemia, 2, birthDate.AddDays(11 * 7), "L-0472", veterinarianId);
        Apply(card, VaccinationCatalogSeeder.FelineRabiesId, 1, birthDate.AddDays(12 * 7), "R-1180",
            veterinarianId);
    }

    private async Task SeedVisitsAsync(Pet rocky, Pet kiara, Guid veterinarianId, DateOnly today,
        CancellationToken cancellationToken)
    {
        var admission = new Visit(rocky.Id, veterinarianId, today.AddDays(-5 * 7), "Control de ingreso",
            "Evaluación general al incorporarse al hogar. Sin hallazgos relevantes.",
            "Cachorro clínicamente sano.", "Sin tratamiento indicado.", 3.6m, today);

        var firstDose = new Visit(rocky.Id, veterinarianId, today.AddDays(-3 * 7),
            "Primera consulta y primera dosis del esquema",
            "Mucosas rosadas, temperatura 38,6 °C. Ganancia de peso acorde a la edad.",
            "Cachorro clínicamente sano, en curso de su esquema de vacunación.",
            "Desparasitación oral y control en tres semanas para la segunda dosis.", 4.2m, today);
        firstDose.IssuePrescription(new[]
        {
            new PrescriptionItem("Praziquantel 50 mg", "Media tableta en dosis única.", "Dosis única")
        });

        var dermatitis = new Visit(kiara.Id, veterinarianId, today.AddDays(-14),
            "Enrojecimiento y rascado en el pliegue facial",
            "Pliegue facial con eritema leve, sin secreción.",
            "Dermatitis del pliegue facial, de grado leve.",
            "Limpieza diaria del pliegue con solución antiséptica y control en catorce días.", 6.8m, today);
        dermatitis.IssuePrescription(new[]
        {
            new PrescriptionItem("Clorhexidina 2% solución",
                "Aplicar en el pliegue dos veces al día.", "10 días"),
            new PrescriptionItem("Cefalexina 250 mg", "Media tableta cada 12 horas.", "7 días")
        });

        await context.Visits.AddRangeAsync([admission, firstDose, dermatitis], cancellationToken);
    }
}
