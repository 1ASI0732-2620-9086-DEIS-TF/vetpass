namespace VetPass.AcceptanceTests.Steps;

[Binding]
public sealed class PatientSteps(ScenarioState state)
{
    [When("the staff searches for patients with {string}")]
    public async Task WhenTheStaffSearches(string criterion) =>
        await state.KeepAsync(await state.Client!.GetAsync($"/api/v1/pets?search={Uri.EscapeDataString(criterion)}"));

    [When("the owner lists their pets")]
    public async Task WhenTheOwnerListsTheirPets() =>
        await state.KeepAsync(await state.Client!.GetAsync("/api/v1/me/pets"));

    [Then("the patients found are {string}")]
    public void ThenThePatientsFoundAre(string names)
    {
        state.Response!.StatusCode.ShouldBe(HttpStatusCode.OK);
        state.Body.EnumerateArray().Select(p => p.GetProperty("name").GetString())
            .ShouldBe(names.Split(", "), ignoreOrder: true);
    }

    [Then("every patient found shows its species and the owner {string}")]
    public void ThenEveryPatientFoundShows(string owner)
    {
        foreach (var patient in state.Body.EnumerateArray())
        {
            patient.GetProperty("species").GetString().ShouldNotBeNullOrWhiteSpace();
            patient.GetProperty("owner").GetProperty("fullName").GetString().ShouldBe(owner);
        }
    }

    [Then("no patient is found")]
    public void ThenNoPatientIsFound()
    {
        state.Response!.StatusCode.ShouldBe(HttpStatusCode.OK);
        state.Body.GetArrayLength().ShouldBe(0);
    }

    [Then("the status of each pet is")]
    public void ThenTheStatusOfEachPetIs(DataTable expected)
    {
        state.Response!.StatusCode.ShouldBe(HttpStatusCode.OK);
        var statuses = state.Body.EnumerateArray().ToDictionary(
            p => p.GetProperty("name").GetString()!,
            p => p.GetProperty("cardStatus").GetString());

        foreach (var row in expected.Rows)
            statuses[row["pet"]].ShouldBe(row["status"], row["pet"]);
    }

    [When("the staff registers the client {string} with DNI {string} and phone {string}")]
    public async Task WhenTheStaffRegistersTheClient(string name, string dni, string phone) =>
        await state.KeepAsync(await state.Client!.PostAsJsonAsync("/api/v1/clients",
            new { fullName = name, documentType = "Dni", documentNumber = dni, phoneNumber = phone }));

    [When("the staff registers the {string} {string} born on {string} for {string}")]
    public async Task WhenTheStaffRegistersThePet(string species, string name, string birth, string owner) =>
        await state.KeepAsync(await state.Client!.PostAsJsonAsync("/api/v1/pets", new
        {
            clientId = await Hooks.ClientIdAsync(owner),
            name,
            species,
            breed = "Mestizo",
            sex = "Male",
            birthDate = birth
        }));

    [Given("the {string} {string} born on {string} is registered for {string}")]
    public async Task GivenThePetIsRegistered(string species, string name, string birth, string owner)
    {
        await WhenTheStaffRegistersThePet(species, name, birth, owner);
        state.Response!.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Then("the client is registered")]
    public void ThenTheClientIsRegistered() => state.Response!.StatusCode.ShouldBe(HttpStatusCode.Created);

    [Then("the phone of the client is stored as {string}")]
    public void ThenThePhoneIsStoredAs(string phone) => state.Body.GetProperty("phoneNumber").GetString().ShouldBe(phone);

    [Then("the request is rejected as a duplicate of the client {string}")]
    public void ThenTheRequestIsADuplicate(string existing)
    {
        state.Response!.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        state.Body.GetProperty("existingClientName").GetString().ShouldBe(existing);
    }

    [Then("the pet is registered with its vaccination card")]
    public async Task ThenThePetIsRegisteredWithItsCard()
    {
        state.Response!.StatusCode.ShouldBe(HttpStatusCode.Created);
        var petId = state.Body.GetProperty("id").GetGuid();

        (await state.Client!.GetAsync($"/api/v1/pets/{petId}/vaccination-card")).StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
