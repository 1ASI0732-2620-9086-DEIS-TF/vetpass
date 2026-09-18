using VetPass.API.Patients.Domain.Model.ValueObjects;

namespace VetPass.API.Vaccination.Domain.Model.Entities;

/// <summary>
/// A vaccine available for a species. Core vaccines correspond to every animal
/// of the species; non-core ones depend on its exposure.
/// </summary>
public class Vaccine
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public Species Species { get; private set; }
    public bool IsCore { get; private set; }

    // Required by Entity Framework Core.
    private Vaccine() { }

    public Vaccine(Guid id, string name, Species species, bool isCore)
    {
        Id = id;
        Name = name;
        Species = species;
        IsCore = isCore;
    }
}
