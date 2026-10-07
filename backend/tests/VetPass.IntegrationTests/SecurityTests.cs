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
    public async Task StaffOfAnotherClinic_CannotSeeItsPatients()
    {
        var rocky = await PetIdAsync(await StaffAsync(), "Rocky");
        var otherClinic = new Clinic(Guid.NewGuid(), "Veterinaria Ajena", "Av. Siempre Viva 123");
        var account = await Api.Identity.CreateAccountAsync("staff@ajena.pe", "Clave2026", Role.ClinicStaff, otherClinic.Id, null);
        await Api.WithDatabaseAsync(async db =>
        {
            db.Clinics.Add(otherClinic);
            db.UserProfiles.Add(new UserProfile(account.Id, "staff@ajena.pe", "Staff Ajeno", Role.ClinicStaff, otherClinic.Id, null));
            await db.SaveChangesAsync();
        });
        var stranger = await Api.SignInAsync("staff@ajena.pe", "Clave2026");

        (await stranger.GetAsync($"/api/v1/pets/{rocky}/vaccination-card")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await stranger.GetFromJsonAsync<JsonElement>("/api/v1/pets")).GetArrayLength().ShouldBe(0);
    }
}
