[assembly: AssemblyFixture(typeof(VetPass.IntegrationTests.ApiFixture))]
// Una sola colección: las pruebas comparten la base y se ejecutan una tras otra.
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]

namespace VetPass.IntegrationTests;

/// <summary>One API and one PostgreSQL for the whole run; each test resets the data.</summary>
public sealed class ApiFixture : IAsyncLifetime
{
    public VetPassApiFactory Api { get; } = new();

    public async ValueTask InitializeAsync() => await Api.StartAsync();

    public ValueTask DisposeAsync() => Api.DisposeAsync();
}

public abstract class ApiTest(ApiFixture fixture) : IAsyncLifetime
{
    protected VetPassApiFactory Api => fixture.Api;

    public async ValueTask InitializeAsync() => await Api.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    protected Task<HttpClient> StaffAsync() => Api.SignInAsync(VetPassApiFactory.StaffEmail);

    protected Task<HttpClient> OwnerAsync() => Api.SignInAsync(VetPassApiFactory.OwnerEmail);

    protected static async Task<JsonElement> JsonAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();

    protected static async Task<Guid> PetIdAsync(HttpClient staff, string name)
    {
        var pets = await staff.GetFromJsonAsync<JsonElement>($"/api/v1/pets?search={name}");
        return pets.EnumerateArray().First(p => p.GetProperty("name").GetString() == name).GetProperty("id").GetGuid();
    }

    protected static async Task<JsonElement> CardAsync(HttpClient client, Guid petId) =>
        await client.GetFromJsonAsync<JsonElement>($"/api/v1/pets/{petId}/vaccination-card");

    protected static JsonElement DoseOf(JsonElement card, string vaccine, int sequence) =>
        card.GetProperty("doses").EnumerateArray().First(d =>
            d.GetProperty("vaccineName").GetString() == vaccine && d.GetProperty("sequenceNumber").GetInt32() == sequence);

    protected static object NewClient(string document, string phone = "912345678") => new
    {
        fullName = "Cliente de Prueba", documentType = "Dni", documentNumber = document, phoneNumber = phone, email = (string?)null
    };
}
