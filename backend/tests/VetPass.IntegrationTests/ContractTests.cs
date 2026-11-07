namespace VetPass.IntegrationTests;

/// <summary>
/// The shape of the answers the web and mobile applications rely on: the
/// ProblemDetails of each rule and the fields of the vaccination card.
/// </summary>
public class ContractTests(ApiFixture fixture) : ApiTest(fixture)
{
    [Fact]
    public async Task MinimumAgeNotReached_CarriesTheRequiredAgeAndTheEarliestDate()
    {
        var staff = await StaffAsync();
        var rocky = await PetIdAsync(staff, "Rocky");
        var rabies = DoseOf(await CardAsync(staff, rocky), "Antirrábica", 1).GetProperty("id").GetGuid();

        var response = await staff.PostAsJsonAsync($"/api/v1/pets/{rocky}/vaccination-card/doses/{rabies}/application",
            new { applicationDate = VetPassApiFactory.Today, batchCode = "R-1300" });

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problem = await JsonAsync(response);
        problem.GetProperty("code").GetString().ShouldBe("minimum-age-not-reached");
        problem.GetProperty("ageInWeeks").GetInt32().ShouldBe(9);
        problem.GetProperty("requiredWeeks").GetInt32().ShouldBe(12);
        problem.GetProperty("earliestAdmissibleDate").GetString().ShouldBe("2026-10-28");
        problem.GetProperty("detail").GetString().ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task DoseOutOfOrder_NamesThePendingDose()
    {
        var staff = await StaffAsync();
        var rocky = await PetIdAsync(staff, "Rocky");
        var third = DoseOf(await CardAsync(staff, rocky), "Quíntuple", 3).GetProperty("id").GetGuid();

        var response = await staff.PostAsJsonAsync($"/api/v1/pets/{rocky}/vaccination-card/doses/{third}/application",
            new { applicationDate = VetPassApiFactory.Today, batchCode = "A-1" });

        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var problem = await JsonAsync(response);
        problem.GetProperty("code").GetString().ShouldBe("dose-out-of-order");
        problem.GetProperty("pendingSequenceNumber").GetInt32().ShouldBe(2);
    }

    [Theory]
    [InlineData("12345", "Dni", "45879999", "invalid-phone-number")]
    [InlineData("912345678", "Dni", "1234567", "invalid-identity-document")]
    public async Task InvalidData_IsABadRequestWithItsCode(string phone, string documentType, string document, string code)
    {
        var staff = await StaffAsync();

        var response = await staff.PostAsJsonAsync("/api/v1/clients", new
        {
            fullName = "Cliente", documentType, documentNumber = document, phoneNumber = phone
        });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await JsonAsync(response)).GetProperty("code").GetString().ShouldBe(code);
    }

    [Fact]
    public async Task VaccinationCard_ExposesWhatTheInterfacesShow()
    {
        var staff = await StaffAsync();
        var card = await CardAsync(staff, await PetIdAsync(staff, "Rocky"));

        card.GetProperty("status").GetString().ShouldBe("Pending");
        card.GetProperty("nextDose").GetProperty("expectedDate").GetString().ShouldBe("2026-10-07");

        var second = DoseOf(card, "Quíntuple", 2);
        second.GetProperty("isNextInSequence").GetBoolean().ShouldBeTrue();
        second.GetProperty("earliestAdmissibleDate").GetString().ShouldBe("2026-10-07");
        second.GetProperty("isBooster").GetBoolean().ShouldBeFalse();
        DoseOf(card, "Quíntuple", 3).GetProperty("isNextInSequence").GetBoolean().ShouldBeFalse();
    }
}
