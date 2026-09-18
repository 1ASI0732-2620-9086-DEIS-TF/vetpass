namespace VetPass.API.IAM.Domain.Model.Aggregates;

/// <summary>Veterinary establishment that operates the platform.</summary>
public class Clinic
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Address { get; private set; }

    // Required by Entity Framework Core.
    private Clinic() { }

    public Clinic(Guid id, string name, string? address)
    {
        Id = id;
        Name = name;
        Address = address;
    }
}
