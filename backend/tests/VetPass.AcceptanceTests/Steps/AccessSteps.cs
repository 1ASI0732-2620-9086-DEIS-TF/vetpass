using VetPass.API.IAM.Domain.Model.Aggregates;
using VetPass.API.IAM.Domain.Model.ValueObjects;

namespace VetPass.AcceptanceTests.Steps;

[Binding]
public sealed class AccessSteps(ScenarioState state)
{
    private static VetPassApiFactory Api => Hooks.Api;

    [Given("the clinic staff is signed in")]
    public async Task GivenTheClinicStaffIsSignedIn() =>
        state.Client = await Api.SignInAsync(VetPassApiFactory.StaffEmail);

    [Given("the owner {string} is signed in")]
    public async Task GivenTheOwnerIsSignedIn(string email) => state.Client = await Api.SignInAsync(email);

    [Given("the staff of another clinic is signed in")]
    public async Task GivenTheStaffOfAnotherClinicIsSignedIn()
    {
        var clinic = new Clinic(Guid.NewGuid(), "Veterinaria Ajena", null);
        var account = await Api.Identity.CreateAccountAsync("staff@ajena.pe", "Clave2026", Role.ClinicStaff, clinic.Id, null);
        await Api.WithDatabaseAsync(async db =>
        {
            db.Clinics.Add(clinic);
            db.UserProfiles.Add(new UserProfile(account.Id, "staff@ajena.pe", "Staff Ajeno", Role.ClinicStaff, clinic.Id, null));
            await db.SaveChangesAsync();
        });
        state.Client = await Api.SignInAsync("staff@ajena.pe", "Clave2026");
    }

    [Given("today is {string}")]
    public void GivenTodayIs(string date) =>
        Api.Clock.SetUtcNow(DateOnly.Parse(date).ToDateTime(new TimeOnly(15, 0), DateTimeKind.Utc));

    [When("the owner {string} signs in with the password given by the clinic")]
    public async Task WhenTheOwnerSignsIn(string email) =>
        await state.KeepAsync(await Api.CreateClient().PostAsJsonAsync("/api/v1/authentication/sign-in",
            new { email, password = VetPassApiFactory.DemoPassword }));

    [When("the user requests the vaccination card of {string}")]
    public async Task WhenTheUserRequestsTheCardOf(string pet) =>
        await state.KeepAsync(await state.Client!.GetAsync($"/api/v1/pets/{await Hooks.PetIdAsync(pet)}/vaccination-card"));

    [Then("the session has the role {string}")]
    public void ThenTheSessionHasTheRole(string role)
    {
        state.Response!.StatusCode.ShouldBe(HttpStatusCode.OK);
        state.Body.GetProperty("user").GetProperty("role").GetString().ShouldBe(role);
    }

    [Then("the owner sees the pets {string}")]
    public async Task ThenTheOwnerSeesThePets(string names)
    {
        var client = Api.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new("Bearer", state.Body.GetProperty("accessToken").GetString());

        var pets = await client.GetFromJsonAsync<JsonElement>("/api/v1/me/pets");

        pets.EnumerateArray().Select(p => p.GetProperty("name").GetString())
            .ShouldBe(names.Split(", "), ignoreOrder: true);
    }

    [Then("the request is denied")]
    public void ThenTheRequestIsDenied() => state.Response!.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

    [Then("the request is rejected with status {int}")]
    public void ThenTheRequestIsRejectedWithStatus(int status) => ((int)state.Response!.StatusCode).ShouldBe(status);

    [Then("the request is rejected with the code {string}")]
    public void ThenTheRequestIsRejectedWithTheCode(string code)
    {
        ((int)state.Response!.StatusCode).ShouldBeInRange(400, 499);
        state.Body.GetProperty("code").GetString().ShouldBe(code);
    }
}
