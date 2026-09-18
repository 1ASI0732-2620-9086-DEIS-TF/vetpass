using VetPass.API.Shared.Domain.Exceptions;

namespace VetPass.API.Patients.Domain.Model.ValueObjects;

/// <summary>
/// Species supported by the platform. The scope declared in section 1.2.1 of
/// the report admits only dogs and cats.
/// </summary>
public enum Species
{
    Canine = 1,
    Feline = 2
}

public static class SpeciesExtensions
{
    /// <summary>
    /// Parses the species received from the outside. Any value other than the
    /// two supported species is rejected (US07-E2).
    /// </summary>
    public static Species ToSpecies(this string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "canine" or "canina" or "canino" or "perro" or "dog" => Species.Canine,
            "feline" or "felina" or "felino" or "gato" or "cat" => Species.Feline,
            _ => throw new UnsupportedSpeciesException(value)
        };
    }
}

public class UnsupportedSpeciesException(string? value)
    : InvalidDomainDataException(
        $"La especie '{value}' no está soportada por la plataforma. Solo se admiten las especies canina y felina.")
{
    public override string Code => "unsupported-species";
}
