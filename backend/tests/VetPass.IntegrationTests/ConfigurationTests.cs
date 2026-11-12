using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VetPass.API.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace VetPass.IntegrationTests;

public class ConfigurationTests(ApiFixture fixture) : ApiTest(fixture)
{
    [Fact]
    public async Task Health_ReportsTheClinicDateAndTimeZone()
    {
        var health = await Api.CreateClient().GetFromJsonAsync<JsonElement>("/api/v1/health", cancellationToken: Ct);

        health.GetProperty("status").GetString().ShouldBe("ok");
        health.GetProperty("today").GetString().ShouldBe("2026-10-07");
        health.GetProperty("timeZone").GetString().ShouldBe("America/Lima");
    }

    [Fact]
    public async Task Readiness_ChecksTheDatabase()
    {
        var response = await Api.CreateClient().GetAsync("/api/v1/health/ready", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await JsonAsync(response)).GetProperty("database").GetString().ShouldBe("ok");
    }

    [Fact]
    public async Task Readiness_IsUnavailableWhenTheDatabaseDoesNotAnswer()
    {
        // La misma API, apuntada a un puerto donde no hay base de datos.
        await using var withoutDatabase = Api.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Database:AutoMigrate", "false");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<VetPassDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<VetPassDbContext>>();
                services.AddDbContext<VetPassDbContext>(options =>
                    options.UseNpgsql("Host=127.0.0.1;Port=1;Database=none;Username=none;Password=none;Timeout=2"));
            });
        });

        var response = await withoutDatabase.CreateClient().GetAsync("/api/v1/health/ready", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
        (await JsonAsync(response)).GetProperty("database").GetString().ShouldBe("unreachable");
    }

    [Theory]
    [InlineData(VetPassApiFactory.AllowedOrigin, true)]
    [InlineData("https://otro-sitio.test", false)]
    public async Task Cors_AllowsOnlyTheWebApplication(string origin, bool allowed)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/pets");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await Api.CreateClient().SendAsync(request, cancellationToken: Ct);

        response.Headers.Contains("Access-Control-Allow-Origin").ShouldBe(allowed);
    }

    [Fact]
    public async Task OpenApiDocument_IsPublished()
    {
        var document = await Api.CreateClient().GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json", cancellationToken: Ct);

        document.GetProperty("paths").EnumerateObject().Count().ShouldBe(17);
    }
}
