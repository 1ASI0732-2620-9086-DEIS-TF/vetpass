namespace VetPass.AcceptanceTests.Steps;

[Binding]
public sealed class PasswordSteps(ScenarioState state)
{
    private static VetPassApiFactory Api => Hooks.Api;

    private static Task<HttpResponseMessage> SignInAsync(string email, string password) =>
        Api.CreateClient().PostAsJsonAsync("/api/v1/authentication/sign-in", new { email, password });

    [When("the owner changes the password to {string}")]
    public async Task WhenTheOwnerChangesThePassword(string password) =>
        await WhenTheOwnerChangesThePasswordGiving(password, state.CurrentPassword);

    [When("the owner changes the password to {string} giving {string} as the current one")]
    public async Task WhenTheOwnerChangesThePasswordGiving(string password, string current) =>
        await state.KeepAsync(await state.Client!.PostAsJsonAsync("/api/v1/authentication/password",
            new { currentPassword = current, newPassword = password }));

    [Then("the change is accepted")]
    public void ThenTheChangeIsAccepted() => state.Response!.StatusCode.ShouldBe(HttpStatusCode.NoContent);

    [Then("the owner signs in with {string} but not with the previous password")]
    public async Task ThenTheOwnerSignsInWithTheNewPassword(string password)
    {
        (await SignInAsync(VetPassApiFactory.OwnerEmail, password)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await SignInAsync(VetPassApiFactory.OwnerEmail, VetPassApiFactory.DemoPassword)).StatusCode
            .ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Then("the session is still open")]
    public async Task ThenTheSessionIsStillOpen() =>
        (await state.Client!.GetAsync("/api/v1/authentication/me")).StatusCode.ShouldBe(HttpStatusCode.OK);

    [Given("the clinic gives mobile access to {string} with the email {string}")]
    public async Task GivenTheClinicGivesMobileAccess(string client, string email)
    {
        var staff = await Api.SignInAsync(VetPassApiFactory.StaffEmail);
        var response = await staff.PostAsJsonAsync("/api/v1/authentication/owner-accounts",
            new { email, fullName = client, clientId = await Hooks.ClientIdAsync(client) });
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        state.TemporaryPassword = (await response.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("temporaryPassword").GetString();
    }

    [When("the owner {string} signs in with the temporary password")]
    public async Task WhenTheOwnerSignsInWithTheTemporaryPassword(string email) =>
        await state.KeepAsync(await SignInAsync(email, state.TemporaryPassword!));

    [Then("the session requires a password change")]
    public void ThenTheSessionRequiresAPasswordChange()
    {
        state.Response!.StatusCode.ShouldBe(HttpStatusCode.OK);
        state.Body.GetProperty("user").GetProperty("requiresPasswordChange").GetBoolean().ShouldBeTrue();
    }

    [When("the staff resets the password of {string}")]
    public async Task WhenTheStaffResetsThePassword(string client)
    {
        await state.KeepAsync(await state.Client!.PostAsync(
            $"/api/v1/authentication/owner-accounts/{await Hooks.ClientIdAsync(client)}/password-reset", null));

        if (state.Response!.IsSuccessStatusCode)
            state.TemporaryPassword = state.Body.GetProperty("temporaryPassword").GetString();
    }

    [Then("a temporary password of {int} characters is issued")]
    public void ThenATemporaryPasswordIsIssued(int length)
    {
        state.Response!.StatusCode.ShouldBe(HttpStatusCode.OK);
        state.TemporaryPassword!.Length.ShouldBe(length);
    }

    [Then("the owner {string} signs in with the temporary password but not with the previous one")]
    public async Task ThenTheOwnerSignsInWithTheTemporaryPassword(string email)
    {
        (await SignInAsync(email, state.TemporaryPassword!)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await SignInAsync(email, VetPassApiFactory.DemoPassword)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [When("the staff lists the clients")]
    public async Task WhenTheStaffListsTheClients() =>
        await state.KeepAsync(await state.Client!.GetAsync("/api/v1/clients"));

    [Then("no client shows a password")]
    public void ThenNoClientShowsAPassword()
    {
        state.Response!.StatusCode.ShouldBe(HttpStatusCode.OK);
        state.Body.EnumerateArray().SelectMany(c => c.EnumerateObject())
            .ShouldNotContain(p => p.Name.Contains("password", StringComparison.OrdinalIgnoreCase));
    }
}
