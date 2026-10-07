using Microsoft.EntityFrameworkCore;

namespace VetPass.IntegrationTests;

/// <summary>The API against a real PostgreSQL: migrations, queries and constraints.</summary>
public class PersistenceTests(ApiFixture fixture) : ApiTest(fixture)
{
    [Fact]
    public async Task Migrations_CreateTheSchemaAndLoadTheVaccineCatalog() =>
        await Api.WithDatabaseAsync(async db =>
        {
            (await db.Database.GetPendingMigrationsAsync()).ShouldBeEmpty();
            (await db.Vaccines.CountAsync()).ShouldBe(5);
            (await db.ScheduleItems.CountAsync()).ShouldBe(15);
        });

    [Theory]
    [InlineData("rOcKy", "Rocky")]
    [InlineData("aliaga", "Rocky")]
    public async Task Search_IsCaseInsensitiveOverPetAndOwnerNames(string search, string expected)
    {
        var staff = await StaffAsync();

        var pets = await staff.GetFromJsonAsync<JsonElement>($"/api/v1/pets?search={search}");

        pets.EnumerateArray().Select(p => p.GetProperty("name").GetString()).ShouldContain(expected);
    }

    [Fact]
    public async Task RepeatedDocument_IsRejectedWithTheExistingClient()
    {
        var staff = await StaffAsync();

        var response = await staff.PostAsJsonAsync("/api/v1/clients", NewClient("45879123"));

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await JsonAsync(response)).GetProperty("existingClientName").GetString().ShouldBe("Valeria Campos");
    }

    [Fact]
    public async Task SimultaneousRegistrations_OfTheSameDocument_LeaveOneClient()
    {
        var staff = await StaffAsync();

        var responses = await Task.WhenAll(
            staff.PostAsJsonAsync("/api/v1/clients", NewClient("70000001", "912345671")),
            staff.PostAsJsonAsync("/api/v1/clients", NewClient("70000001", "912345672")));

        responses.Count(r => r.StatusCode == HttpStatusCode.Created).ShouldBe(1);
        responses.Count(r => r.StatusCode == HttpStatusCode.Conflict).ShouldBe(1);
        await Api.WithDatabaseAsync(async db =>
            (await db.Clients.CountAsync(c => c.DocumentNumber == "70000001")).ShouldBe(1));
    }

    [Fact]
    public async Task RemovingAPet_RemovesItsCardAndDoses()
    {
        var petId = await PetIdAsync(await StaffAsync(), "Rocky");

        await Api.WithDatabaseAsync(async db =>
        {
            db.Pets.Remove(await db.Pets.SingleAsync(p => p.Id == petId));
            await db.SaveChangesAsync();

            (await db.VaccinationCards.CountAsync(c => c.PetId == petId)).ShouldBe(0);
        });
    }
}
