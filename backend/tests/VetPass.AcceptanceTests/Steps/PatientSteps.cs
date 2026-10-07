namespace VetPass.AcceptanceTests.Steps;

[Binding]
public sealed class PatientSteps(ScenarioState state)
{
    [When("the staff registers the client {string} with DNI {string} and phone {string}")]
    public async Task WhenTheStaffRegistersTheClient(string name, string dni, string phone) =>
        await state.KeepAsync(await state.Client!.PostAsJsonAsync("/api/v1/clients",
            new { fullName = name, documentType = "Dni", documentNumber = dni, phoneNumber = phone }));

    [When("the staff registers the {string} {string} born on {string} for {string}")]
    public async Task WhenTheStaffRegistersThePet(string species, string name, string birth, string owner) =>
        await state.KeepAsync(await state.Client!.PostAsJsonAsync("/api/v1/pets", new
        {
            clientId = await Hooks.ClientIdAsync(owner), name, species, breed = "Mestizo", sex = "Male", birthDate = birth
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
