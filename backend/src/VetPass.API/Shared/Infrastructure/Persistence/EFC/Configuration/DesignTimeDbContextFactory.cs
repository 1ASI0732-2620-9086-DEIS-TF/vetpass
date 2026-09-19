using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace VetPass.API.Shared.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Builds the context for the Entity Framework Core tools, without starting the
/// application.
///
/// Creating a migration only needs the model, so a placeholder connection string
/// is enough when none is configured; applying it does need the real one, which
/// is read from user secrets or from the environment and never from the
/// repository.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<VetPassDbContext>
{
    private const string PlaceholderConnection =
        "Host=localhost;Port=5432;Database=vetpass;Username=postgres;Password=postgres";

    public VetPassDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<DesignTimeDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("VetPassDb") ?? PlaceholderConnection;

        var options = new DbContextOptionsBuilder<VetPassDbContext>()
            .UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", VetPassDbContext.Schema))
            .Options;

        return new VetPassDbContext(options);
    }
}
