namespace VetPass.AcceptanceTests.Steps;

/// <summary>What one step leaves for the next ones within a scenario.</summary>
public sealed class ScenarioState
{
    public HttpClient? Client { get; set; }
    public HttpResponseMessage? Response { get; private set; }
    public JsonElement Body { get; private set; }
    public string CurrentPassword { get; set; } = VetPassApiFactory.DemoPassword;
    public string? TemporaryPassword { get; set; }
    public (Guid Pet, string Vaccine, int Dose)? LastDose { get; set; }

    public async Task KeepAsync(HttpResponseMessage response)
    {
        Response = response;
        var text = await response.Content.ReadAsStringAsync();
        Body = string.IsNullOrWhiteSpace(text) ? default : JsonDocument.Parse(text).RootElement.Clone();
    }
}
