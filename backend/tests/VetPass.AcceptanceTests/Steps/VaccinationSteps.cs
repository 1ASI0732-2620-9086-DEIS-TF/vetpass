namespace VetPass.AcceptanceTests.Steps;

[Binding]
public sealed class VaccinationSteps(ScenarioState state)
{
    private async Task<JsonElement> CardAsync(string pet) =>
        await state.Client!.GetFromJsonAsync<JsonElement>($"/api/v1/pets/{await Hooks.PetIdAsync(pet)}/vaccination-card");

    private static JsonElement DoseOf(JsonElement card, string vaccine, int dose) =>
        card.GetProperty("doses").EnumerateArray().First(d =>
            d.GetProperty("vaccineName").GetString() == vaccine && d.GetProperty("sequenceNumber").GetInt32() == dose);

    [When("the staff registers dose {int} of {string} for {string} applied on {string} with batch {string}")]
    public async Task WhenTheStaffRegistersADose(int dose, string vaccine, string pet, string date, string batch)
    {
        var petId = await Hooks.PetIdAsync(pet);
        var doseId = DoseOf(await CardAsync(pet), vaccine, dose).GetProperty("id").GetGuid();
        state.LastDose = (petId, vaccine, dose);

        await state.KeepAsync(await state.Client!.PostAsJsonAsync(
            $"/api/v1/pets/{petId}/vaccination-card/doses/{doseId}/application",
            new { applicationDate = date, batchCode = batch }));
    }

    [Given("dose {int} of {string} for {string} was applied on {string}")]
    public async Task GivenADoseWasApplied(int dose, string vaccine, string pet, string date)
    {
        await WhenTheStaffRegistersADose(dose, vaccine, pet, date, "A-0001");
        state.Response!.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Then("the dose is registered as applied with batch {string}")]
    public async Task ThenTheDoseIsApplied(string batch)
    {
        state.Response!.StatusCode.ShouldBe(HttpStatusCode.Created);
        var (petId, vaccine, dose) = state.LastDose!.Value;
        var card = await state.Client!.GetFromJsonAsync<JsonElement>($"/api/v1/pets/{petId}/vaccination-card");

        var applied = DoseOf(card, vaccine, dose);
        applied.GetProperty("status").GetString().ShouldBe("Applied");
        applied.GetProperty("batchCode").GetString().ShouldBe(batch);
    }

    [Then("the earliest admissible date is {string}")]
    public void ThenTheEarliestAdmissibleDateIs(string date) =>
        state.Body.GetProperty("earliestAdmissibleDate").GetString().ShouldBe(date);

    [Then("the card of {string} has {int} pending doses")]
    public async Task ThenTheCardHasPendingDoses(string pet, int doses)
    {
        var card = await CardAsync(pet);

        card.GetProperty("doses").GetArrayLength().ShouldBe(doses);
        card.GetProperty("doses").EnumerateArray().ShouldAllBe(d => d.GetProperty("status").GetString() == "Pending");
    }

    [Then("the doses of {string} are expected on")]
    public async Task ThenTheDosesAreExpectedOn(string pet, DataTable expected)
    {
        var card = await CardAsync(pet);

        foreach (var row in expected.Rows)
            DoseOf(card, row["vaccine"], int.Parse(row["dose"])).GetProperty("expectedDate").GetString()
                .ShouldBe(row["date"], $"{row["vaccine"]} {row["dose"]}");
    }

    [Then("the status of the card is {string}")]
    public void ThenTheStatusOfTheCardIs(string status) => state.Body.GetProperty("status").GetString().ShouldBe(status);

    [Then("the next dose is dose {int} of {string}")]
    public void ThenTheNextDoseIs(int dose, string vaccine)
    {
        var next = state.Body.GetProperty("nextDose");
        next.GetProperty("vaccineName").GetString().ShouldBe(vaccine);
        next.GetProperty("sequenceNumber").GetInt32().ShouldBe(dose);
    }
}
