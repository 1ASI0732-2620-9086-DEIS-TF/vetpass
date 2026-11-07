namespace VetPass.IntegrationTests;

/// <summary>Complete flows across the four bounded contexts.</summary>
public class FlowTests(ApiFixture fixture) : ApiTest(fixture)
{
    [Fact]
    public async Task ClientPetCardDoseAndVisitWithPrescription()
    {
        var staff = await StaffAsync();

        var client = await JsonAsync(await staff.PostAsJsonAsync("/api/v1/clients", NewClient("70000010")));
        var pet = await staff.PostAsJsonAsync("/api/v1/pets", new
        {
            clientId = client.GetProperty("id").GetGuid(), name = "Nube", species = "Canine", breed = "Mestizo",
            sex = "Female", birthDate = VetPassApiFactory.Today.AddDays(-6 * 7)
        });
        pet.StatusCode.ShouldBe(HttpStatusCode.Created);
        var petId = (await JsonAsync(pet)).GetProperty("id").GetGuid();

        var card = await CardAsync(staff, petId);
        card.GetProperty("doses").GetArrayLength().ShouldBe(6);
        card.GetProperty("status").GetString().ShouldBe("Pending");

        var first = DoseOf(card, "Quíntuple", 1).GetProperty("id").GetGuid();
        (await staff.PostAsJsonAsync($"/api/v1/pets/{petId}/vaccination-card/doses/{first}/application",
            new { applicationDate = VetPassApiFactory.Today, batchCode = "a-4521" })).StatusCode.ShouldBe(HttpStatusCode.Created);

        card = await CardAsync(staff, petId);
        card.GetProperty("status").GetString().ShouldBe("UpToDate");
        DoseOf(card, "Quíntuple", 1).GetProperty("batchCode").GetString().ShouldBe("A-4521");

        var visit = await staff.PostAsJsonAsync($"/api/v1/pets/{petId}/visits", new
        {
            visitDate = VetPassApiFactory.Today, reason = "Primera consulta", findings = "Sana", diagnosis = "Cachorro sano",
            treatment = "Desparasitación", weightKg = 3.4m,
            prescription = new[] { new { medication = "Praziquantel 50 mg", dosage = "Una tableta", duration = "Dosis única" } }
        });
        visit.StatusCode.ShouldBe(HttpStatusCode.Created);

        var history = await staff.GetFromJsonAsync<JsonElement>($"/api/v1/pets/{petId}/visits");
        history.GetArrayLength().ShouldBe(1);
    }

    [Fact]
    public async Task OwnerAccess_TemporaryPasswordAndChange()
    {
        var staff = await StaffAsync();
        var jorge = (await staff.GetFromJsonAsync<JsonElement>("/api/v1/clients"))
            .EnumerateArray().First(c => c.GetProperty("fullName").GetString() == "Jorge Aliaga");

        var created = await JsonAsync(await staff.PostAsJsonAsync("/api/v1/authentication/owner-accounts", new
        {
            email = "jorge.aliaga@correo.com", fullName = "Jorge Aliaga", clientId = jorge.GetProperty("id").GetGuid()
        }));
        var temporary = created.GetProperty("temporaryPassword").GetString()!;
        created.GetProperty("user").GetProperty("requiresPasswordChange").GetBoolean().ShouldBeTrue();

        var owner = await Api.SignInAsync("jorge.aliaga@correo.com", temporary);
        (await owner.PostAsJsonAsync("/api/v1/authentication/password",
            new { currentPassword = temporary, newPassword = "Perrito2026" })).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var me = await (await Api.SignInAsync("jorge.aliaga@correo.com", "Perrito2026"))
            .GetFromJsonAsync<JsonElement>("/api/v1/authentication/me");
        me.GetProperty("requiresPasswordChange").GetBoolean().ShouldBeFalse();
    }

    [Fact]
    public async Task PasswordReset_ReplacesTheForgottenPassword()
    {
        var staff = await StaffAsync();
        var valeria = (await staff.GetFromJsonAsync<JsonElement>("/api/v1/clients"))
            .EnumerateArray().First(c => c.GetProperty("fullName").GetString() == "Valeria Campos").GetProperty("id").GetGuid();

        var reset = await JsonAsync(await staff.PostAsync($"/api/v1/authentication/owner-accounts/{valeria}/password-reset", null));
        var temporary = reset.GetProperty("temporaryPassword").GetString()!;

        (await Api.CreateClient().PostAsJsonAsync("/api/v1/authentication/sign-in",
            new { email = VetPassApiFactory.OwnerEmail, password = VetPassApiFactory.DemoPassword }))
            .StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        await Api.SignInAsync(VetPassApiFactory.OwnerEmail, temporary);
    }

    [Fact]
    public async Task RefreshToken_RenewsTheSession()
    {
        var signIn = await JsonAsync(await Api.CreateClient().PostAsJsonAsync("/api/v1/authentication/sign-in",
            new { email = VetPassApiFactory.OwnerEmail, password = VetPassApiFactory.DemoPassword }));

        var refreshed = await Api.CreateClient().PostAsJsonAsync("/api/v1/authentication/refresh",
            new { refreshToken = signIn.GetProperty("refreshToken").GetString() });

        refreshed.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await JsonAsync(refreshed)).GetProperty("accessToken").GetString().ShouldNotBeNullOrWhiteSpace();
    }
}
