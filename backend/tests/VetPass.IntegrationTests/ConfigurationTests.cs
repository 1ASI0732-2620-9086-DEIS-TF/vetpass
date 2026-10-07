namespace VetPass.IntegrationTests;

public class ConfigurationTests(ApiFixture fixture) : ApiTest(fixture)
{
    [Fact]
    public async Task Health_ReportsTheClinicDateAndTimeZone()
    {
        var health = await Api.CreateClient().GetFromJsonAsync<JsonElement>("/api/v1/health");

        health.GetProperty("status").GetString().ShouldBe("ok");
        health.GetProperty("today").GetString().ShouldBe("2026-10-07");
        health.GetProperty("timeZone").GetString().ShouldBe("America/Lima");
    }

    [Theory]
    [InlineData(VetPassApiFactory.AllowedOrigin, true)]
    [InlineData("https://otro-sitio.test", false)]
    public async Task Cors_AllowsOnlyTheWebApplication(string origin, bool allowed)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/pets");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await Api.CreateClient().SendAsync(request);

        response.Headers.Contains("Access-Control-Allow-Origin").ShouldBe(allowed);
    }

    [Fact]
    public async Task OpenApiDocument_IsPublished()
    {
        var document = await Api.CreateClient().GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json");

        document.GetProperty("paths").EnumerateObject().Count().ShouldBe(17);
    }
}
