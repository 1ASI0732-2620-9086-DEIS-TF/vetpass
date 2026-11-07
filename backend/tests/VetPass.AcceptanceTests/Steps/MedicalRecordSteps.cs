namespace VetPass.AcceptanceTests.Steps;

[Binding]
public sealed class MedicalRecordSteps(ScenarioState state)
{
    private static VetPassApiFactory Api => Hooks.Api;

    private async Task<JsonElement> HistoryAsync(HttpClient client, string pet) =>
        await client.GetFromJsonAsync<JsonElement>($"/api/v1/pets/{await Hooks.PetIdAsync(pet)}/visits");

    private async Task RegisterVisitAsync(string pet, string date, string reason, string diagnosis) =>
        await state.KeepAsync(await state.Client!.PostAsJsonAsync($"/api/v1/pets/{await Hooks.PetIdAsync(pet)}/visits",
            new { visitDate = date, reason, diagnosis }));

    [When("the staff registers a visit for {string} on {string} with reason {string} and diagnosis {string}")]
    public async Task WhenTheStaffRegistersAVisit(string pet, string date, string reason, string diagnosis) =>
        await RegisterVisitAsync(pet, date, reason, diagnosis);

    [Given("the staff registered a visit for {string} on {string} with reason {string} and diagnosis {string}")]
    public async Task GivenTheStaffRegisteredAVisit(string pet, string date, string reason, string diagnosis)
    {
        await RegisterVisitAsync(pet, date, reason, diagnosis);
        state.Response!.StatusCode.ShouldBe(HttpStatusCode.Created);
        state.LastVisit = state.Body.GetProperty("id").GetGuid();
    }

    [When("the user requests the history of {string}")]
    public async Task WhenTheUserRequestsTheHistory(string pet) =>
        await state.KeepAsync(await state.Client!.GetAsync($"/api/v1/pets/{await Hooks.PetIdAsync(pet)}/visits"));

    [When("the user opens the visit of {string} with reason {string}")]
    public async Task WhenTheUserOpensTheVisit(string pet, string reason)
    {
        var visit = (await HistoryAsync(state.Client!, pet)).EnumerateArray()
            .First(v => v.GetProperty("reason").GetString() == reason);

        await state.KeepAsync(await state.Client!.GetAsync($"/api/v1/visits/{visit.GetProperty("id").GetGuid()}"));
    }

    [When("the staff issues a prescription of {string} with dosage {string} for {string}")]
    public async Task WhenTheStaffIssuesAPrescription(string medication, string dosage, string duration) =>
        await state.KeepAsync(await state.Client!.PostAsJsonAsync($"/api/v1/visits/{state.LastVisit}/prescription",
            new { items = new[] { new { medication, dosage, duration } } }));

    [When("the staff issues a prescription without medications")]
    public async Task WhenTheStaffIssuesAnEmptyPrescription() =>
        await state.KeepAsync(await state.Client!.PostAsJsonAsync($"/api/v1/visits/{state.LastVisit}/prescription",
            new { items = Array.Empty<object>() }));

    [Then("the visit is in the history of {string} on {string} with the staff as responsible")]
    public async Task ThenTheVisitIsInTheHistory(string pet, string date)
    {
        state.Response!.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = state.Body.GetProperty("id").GetGuid();

        var visit = (await HistoryAsync(state.Client!, pet)).EnumerateArray()
            .Single(v => v.GetProperty("id").GetGuid() == created);

        visit.GetProperty("visitDate").GetString().ShouldBe(date);
        var staff = await state.Client!.GetFromJsonAsync<JsonElement>("/api/v1/authentication/me");
        visit.GetProperty("veterinarianId").GetGuid().ShouldBe(staff.GetProperty("id").GetGuid());
    }

    [Then("the visits are, from the most recent")]
    public void ThenTheVisitsAre(DataTable expected)
    {
        state.Response!.StatusCode.ShouldBe(HttpStatusCode.OK);
        state.Body.EnumerateArray().Select(v => v.GetProperty("reason").GetString())
            .ShouldBe(expected.Rows.Select(row => row["reason"]));
    }

    [Then("the history has no visits")]
    public void ThenTheHistoryHasNoVisits()
    {
        state.Response!.StatusCode.ShouldBe(HttpStatusCode.OK);
        state.Body.GetArrayLength().ShouldBe(0);
    }

    [Then("the prescription is issued")]
    public void ThenThePrescriptionIsIssued() => state.Response!.IsSuccessStatusCode.ShouldBeTrue();

    [Then("the owner {string} sees {string} in the prescription of the visit")]
    public async Task ThenTheOwnerSeesThePrescription(string email, string medication)
    {
        var owner = await Api.SignInAsync(email);

        var visit = await owner.GetFromJsonAsync<JsonElement>($"/api/v1/visits/{state.LastVisit}");

        visit.GetProperty("prescription").GetProperty("items").EnumerateArray()
            .Select(item => item.GetProperty("medication").GetString())
            .ShouldContain(medication);
    }

    [Then("the prescription contains")]
    public void ThenThePrescriptionContains(DataTable expected)
    {
        state.Response!.StatusCode.ShouldBe(HttpStatusCode.OK);
        var items = state.Body.GetProperty("prescription").GetProperty("items").EnumerateArray()
            .Select(item => (
                item.GetProperty("medication").GetString(),
                item.GetProperty("dosage").GetString(),
                item.GetProperty("duration").GetString()));

        items.ShouldBe(expected.Rows.Select(row => ((string?)row["medication"], (string?)row["dosage"], (string?)row["duration"])),
            ignoreOrder: true);
    }
}
