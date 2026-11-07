using Microsoft.EntityFrameworkCore;

// Los escenarios comparten una base: se ejecutan uno tras otro.
[assembly: Xunit.CollectionBehavior(Xunit.CollectionBehavior.CollectionPerAssembly)]

namespace VetPass.AcceptanceTests;

[Binding]
public sealed class Hooks
{
    public static VetPassApiFactory Api { get; private set; } = null!;

    [BeforeTestRun]
    public static async Task StartAsync()
    {
        Api = new VetPassApiFactory();
        await Api.StartAsync();
    }

    [AfterTestRun]
    public static async Task StopAsync() => await Api.DisposeAsync();

    /// <summary>Every scenario starts from the demonstration case of 7 October 2026.</summary>
    [BeforeScenario]
    public static async Task ResetAsync() => await Api.ResetAsync();

    public static async Task<Guid> PetIdAsync(string name)
    {
        var id = Guid.Empty;
        await Api.WithDatabaseAsync(async db => id = (await db.Pets.SingleAsync(p => p.Name == name)).Id);
        return id;
    }

    public static async Task<Guid> ClientIdAsync(string fullName)
    {
        var id = Guid.Empty;
        await Api.WithDatabaseAsync(async db => id = (await db.Clients.SingleAsync(c => c.FullName == fullName)).Id);
        return id;
    }
}
