using VetPass.API.Patients.Domain.Model.ValueObjects;
using VetPass.API.Shared.Domain.Exceptions;

namespace VetPass.API.Patients.Domain.Model.Aggregates;

/// <summary>
/// A pet registered as a patient of the clinic. Aggregate root of the Patients
/// bounded context; the Vaccination and Medical Records contexts consume its
/// identifier, species and birth date without modifying them.
/// </summary>
public class Pet
{
    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }
    public string Name { get; private set; } = null!;
    public Species Species { get; private set; }
    public string? Breed { get; private set; }
    public Sex Sex { get; private set; }
    public DateOnly BirthDate { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Required by Entity Framework Core.
    private Pet() { }

    public Pet(Guid clientId, string name, Species species, string? breed, Sex sex, DateOnly birthDate, DateOnly today)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new RequiredPetFieldException("nombre");

        if (birthDate > today)
            throw new FutureBirthDateException(birthDate, today);

        Id = Guid.NewGuid();
        ClientId = clientId;
        Name = name.Trim();
        Species = species;
        Breed = string.IsNullOrWhiteSpace(breed) ? null : breed.Trim();
        Sex = sex;
        BirthDate = birthDate;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Age in completed weeks. The vaccination schedule expresses its minimum
    /// ages in weeks, so this is the unit the domain reasons in.
    /// </summary>
    public int AgeInWeeks(DateOnly today) => today.DayNumber - BirthDate.DayNumber < 0
        ? 0
        : (today.DayNumber - BirthDate.DayNumber) / 7;
}

public class FutureBirthDateException(DateOnly birthDate, DateOnly today)
    : InvalidDomainDataException(
        $"La fecha de nacimiento {birthDate:dd/MM/yyyy} es posterior a la fecha actual {today:dd/MM/yyyy}.")
{
    public override string Code => "future-birth-date";
}

public class RequiredPetFieldException(string field)
    : InvalidDomainDataException($"El campo '{field}' de la mascota es obligatorio.")
{
    public override string Code => "required-pet-field";
}
