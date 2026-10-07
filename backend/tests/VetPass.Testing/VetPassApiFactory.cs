using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;
using VetPass.API.IAM.Domain.Services;
using VetPass.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using VetPass.API.Shared.Infrastructure.Persistence.EFC.Seeding;

namespace VetPass.Testing;

/// <summary>
/// The RESTful API in memory, on a disposable PostgreSQL. Each test starts from
/// the demonstration case of the seed, on a fixed date, without touching the
/// database or the identity provider of production.
/// </summary>
public sealed class VetPassApiFactory : WebApplicationFactory<Program>
{
    public const string JwtSecret = "vetpass-test-secret-with-at-least-32-bytes";
    public const string SupabaseUrl = "https://supabase.test";
    public const string DemoPassword = "VetPass.Test2026";
    public const string StaffEmail = "andrea.quispe@vetsanmiguel.pe";
    public const string OwnerEmail = "valeria.campos@correo.com";
    public const string AllowedOrigin = "https://vetpass-web.test";

    /// <summary>7 October 2026, 10:00 in Lima: the date the demonstration case is built on.</summary>
    public static readonly DateTimeOffset Now = new(2026, 10, 7, 15, 0, 0, TimeSpan.Zero);
    public static readonly DateOnly Today = new(2026, 10, 7);

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:17-alpine").Build();

    private Respawner? _respawner;

    public FakeTimeProvider Clock { get; } = new(Now);
    public FakeIdentityProvider Identity { get; }

    public VetPassApiFactory() => Identity = new FakeIdentityProvider(Clock);

    public string ConnectionString => _database.GetConnectionString();

    /// <summary>Starts PostgreSQL and the API, which applies the migrations and the vaccine catalog.</summary>
    public async Task StartAsync()
    {
        await _database.StartAsync();

        // The API reads this configuration while it is being built, before the
        // factory can add its own sources: it travels as environment variables.
        Environment.SetEnvironmentVariable("ConnectionStrings__VetPassDb", ConnectionString);
        Environment.SetEnvironmentVariable("Supabase__Url", SupabaseUrl);
        Environment.SetEnvironmentVariable("Supabase__JwtSecret", JwtSecret);
        Environment.SetEnvironmentVariable("Supabase__AnonKey", "test-anon-key");
        Environment.SetEnvironmentVariable("Supabase__ServiceRoleKey", "test-service-role-key");
        Environment.SetEnvironmentVariable("Cors__AllowedOrigins", AllowedOrigin);

        _ = Server;

        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = [VetPassDbContext.Schema],
            TablesToIgnore =
            [
                new Table(VetPassDbContext.Schema, "vaccines"),
                new Table(VetPassDbContext.Schema, "schedule_items"),
                new Table(VetPassDbContext.Schema, "__EFMigrationsHistory"),
            ],
        });
    }

    /// <summary>Empties the clinical data and, unless told otherwise, loads the demonstration case again.</summary>
    public async Task ResetAsync(bool seedDemo = true)
    {
        await using (var connection = new NpgsqlConnection(ConnectionString))
        {
            await connection.OpenAsync();
            await _respawner!.ResetAsync(connection);
        }

        Identity.Clear();
        Clock.SetUtcNow(Now);

        if (!seedDemo) return;

        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<DemoDataSeeder>().SeedAsync(DemoPassword);
    }

    /// <summary>A client of the API with the session of the given user, opened through the API itself.</summary>
    public async Task<HttpClient> SignInAsync(string email, string password = DemoPassword)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/authentication/sign-in", new { email, password });
        response.EnsureSuccessStatusCode();

        var session = await response.Content.ReadFromJsonAsync<SignedIn>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session!.AccessToken);
        return client;
    }

    /// <summary>Direct access to the database, to prepare or inspect what the API cannot.</summary>
    public async Task WithDatabaseAsync(Func<VetPassDbContext, Task> action)
    {
        using var scope = Services.CreateScope();
        await action(scope.ServiceProvider.GetRequiredService<VetPassDbContext>());
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IIdentityProvider>();
            services.AddSingleton<IIdentityProvider>(Identity);
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }

    private sealed record SignedIn(string AccessToken, string RefreshToken);
}
