using System.Net.Http.Headers;
using VetPass.API.IAM.Domain.Model.Aggregates;
using VetPass.API.IAM.Domain.Model.ValueObjects;

namespace VetPass.IntegrationTests;

/// <summary>Authentication and authorisation by role, clinic and owner.</summary>
public class SecurityTests(ApiFixture fixture) : ApiTest(fixture)
{
    [Fact]
    public async Task WithoutToken_IsUnauthorized() =>
        (await Api.CreateClient().GetAsync("/api/v1/pets")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

    [Fact]
    public async Task ForgedToken_IsUnauthorized()
    {
        var client = Api.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", FakeIdentityProvider.ForgedToken());

        (await client.GetAsync("/api/v1/pets")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WrongPassword_IsUnauthorized()
    {
        var response = await Api.CreateClient().PostAsJsonAsync("/api/v1/authentication/sign-in",
            new { email = VetPassApiFactory.StaffEmail, password = "NoEsLaClave1" });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Owner_SeesOnlyTheirOwnPets()
    {
        var rocky = await PetIdAsync(await StaffAsync(), "Rocky");
        var owner = await OwnerAsync();

        var mine = await owner.GetFromJsonAsync<JsonElement>("/api/v1/me/pets");
        mine.EnumerateArray().Select(p => p.GetProperty("name").GetString()).ShouldBe(["Kiara", "Simón"], ignoreOrder: true);

        (await owner.GetAsync($"/api/v1/pets/{rocky}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await owner.GetAsync($"/api/v1/pets/{rocky}/vaccination-card")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await owner.GetAsync($"/api/v1/pets/{rocky}/visits")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Owner_CannotUseTheEndpointsOfTheClinic()
    {
        var owner = await OwnerAsync();

        (await owner.GetAsync("/api/v1/clients")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await owner.PostAsJsonAsync("/api/v1/clients", NewClient("70000002"))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task StaffOfAnotherClinic_CannotReadOrWriteItsData()
    {
        var staff = await StaffAsync();
        var rocky = await PetIdAsync(staff, "Rocky");
        var secondDose = DoseOf(await CardAsync(staff, rocky), "Quíntuple", 2).GetProperty("id").GetGuid();
        var visit = (await staff.GetFromJsonAsync<JsonElement>($"/api/v1/pets/{rocky}/visits"))
            .EnumerateArray().First(v => !v.GetProperty("hasPrescription").GetBoolean()).GetProperty("id").GetGuid();
        var clients = await staff.GetFromJsonAsync<JsonElement>("/api/v1/clients");
        Guid ClientId(string name) => clients.EnumerateArray()
            .First(c => c.GetProperty("fullName").GetString() == name).GetProperty("id").GetGuid();

        var stranger = await StrangerAsync();
        var item = new[] { new { medication = "A", dosage = "B", duration = "C" } };

        var responses = new Dictionary<string, HttpResponseMessage>
        {
            ["GET pet"] = await stranger.GetAsync($"/api/v1/pets/{rocky}"),
            ["GET card"] = await stranger.GetAsync($"/api/v1/pets/{rocky}/vaccination-card"),
            ["GET visits"] = await stranger.GetAsync($"/api/v1/pets/{rocky}/visits"),
            ["GET visit"] = await stranger.GetAsync($"/api/v1/visits/{visit}"),
            ["GET client"] = await stranger.GetAsync($"/api/v1/clients/{ClientId("Jorge Aliaga")}"),
            ["POST dose"] = await stranger.PostAsJsonAsync(
                $"/api/v1/pets/{rocky}/vaccination-card/doses/{secondDose}/application",
                new { applicationDate = VetPassApiFactory.Today, batchCode = "X-1" }),
            ["POST visit"] = await stranger.PostAsJsonAsync($"/api/v1/pets/{rocky}/visits",
                new { visitDate = VetPassApiFactory.Today, reason = "Ajena", diagnosis = "Ajena" }),
            ["POST prescription"] = await stranger.PostAsJsonAsync($"/api/v1/visits/{visit}/prescription", new { items = item }),
            ["POST pet"] = await stranger.PostAsJsonAsync("/api/v1/pets", new
            {
                clientId = ClientId("Jorge Aliaga"), name = "Ajena", species = "Canine", sex = "Male",
                birthDate = VetPassApiFactory.Today.AddDays(-30)
            }),
            ["POST owner account"] = await stranger.PostAsJsonAsync("/api/v1/authentication/owner-accounts",
                new { email = "jorge@ajena.pe", fullName = "Jorge", clientId = ClientId("Jorge Aliaga") }),
            ["POST password reset"] = await stranger.PostAsync(
                $"/api/v1/authentication/owner-accounts/{ClientId("Valeria Campos")}/password-reset", null),
        };

        responses.Where(r => r.Value.StatusCode != HttpStatusCode.Forbidden)
            .Select(r => $"{r.Key}: {(int)r.Value.StatusCode}").ShouldBeEmpty();
        (await stranger.GetFromJsonAsync<JsonElement>("/api/v1/pets")).GetArrayLength().ShouldBe(0);
        DoseOf(await CardAsync(staff, rocky), "Quíntuple", 2).GetProperty("status").GetString().ShouldBe("Pending");
    }

    /// <summary>A member of the staff of another clinic, with its own session.</summary>
    private async Task<HttpClient> StrangerAsync()
    {
        var otherClinic = new Clinic(Guid.NewGuid(), "Veterinaria Ajena", "Av. Siempre Viva 123");
        var account = await Api.Identity.CreateAccountAsync("staff@ajena.pe", "Clave2026", Role.ClinicStaff, otherClinic.Id, null);
        await Api.WithDatabaseAsync(async db =>
        {
            db.Clinics.Add(otherClinic);
            db.UserProfiles.Add(new UserProfile(account.Id, "staff@ajena.pe", "Staff Ajeno", Role.ClinicStaff, otherClinic.Id, null));
            await db.SaveChangesAsync();
        });
        return await Api.SignInAsync("staff@ajena.pe", "Clave2026");
    }
}
