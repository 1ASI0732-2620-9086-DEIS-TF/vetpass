using VetPass.API.Shared.Domain.Exceptions;

namespace VetPass.API.Patients.Domain.Model.ValueObjects;

public enum Sex
{
    Male = 1,
    Female = 2
}

public static class SexExtensions
{
    public static Sex ToSex(this string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "male" or "macho" or "m" => Sex.Male,
            "female" or "hembra" or "f" => Sex.Female,
            _ => throw new UnsupportedSexException(value)
        };
    }
}

public class UnsupportedSexException(string? value)
    : InvalidDomainDataException($"El sexo '{value}' no es válido. Los valores admitidos son macho y hembra.")
{
    public override string Code => "unsupported-sex";
}
