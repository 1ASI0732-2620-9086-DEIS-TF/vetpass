using System.Text.RegularExpressions;
using VetPass.API.Shared.Domain.Exceptions;

namespace VetPass.API.Patients.Domain.Model.ValueObjects;

/// <summary>Identity documents admitted for a client of the clinic.</summary>
public enum IdentityDocumentType
{
    /// <summary>Documento Nacional de Identidad, of Peruvian citizens.</summary>
    Dni = 1,

    /// <summary>Carné de Extranjería, of foreign residents.</summary>
    ForeignerCard = 2
}

/// <summary>
/// Document that identifies a client and makes it impossible to register the
/// same person twice in a clinic. Both Peruvian citizens and foreign residents
/// are admitted, because a large share of the population of Lima holds a
/// Carné de Extranjería and not a DNI.
///
/// <list type="bullet">
///   <item>DNI: exactly eight digits.</item>
///   <item>Carné de Extranjería: between nine and twelve letters or digits.</item>
/// </list>
/// Spaces and dashes written by the user are discarded, and letters are kept in
/// upper case, so that two ways of writing the same document are one value.
/// </summary>
public sealed partial record IdentityDocument
{
    public IdentityDocumentType Type { get; }
    public string Number { get; }

    private IdentityDocument(IdentityDocumentType type, string number)
    {
        Type = type;
        Number = number;
    }

    public static IdentityDocument Of(IdentityDocumentType type, string? number)
    {
        var written = Separators().Replace(number ?? string.Empty, string.Empty).ToUpperInvariant();

        var valid = type switch
        {
            IdentityDocumentType.Dni => Dni().IsMatch(written),
            IdentityDocumentType.ForeignerCard => ForeignerCard().IsMatch(written),
            _ => false
        };

        if (!valid) throw new InvalidIdentityDocumentException(type, number);

        return new IdentityDocument(type, written);
    }

    /// <summary>
    /// Builds the document from the text an interface sends: the type admits
    /// its name in the domain and the abbreviations the clinic uses.
    /// </summary>
    public static IdentityDocument Parse(string? type, string? number) =>
        Of(ParseType(type), number);

    public static IdentityDocumentType ParseType(string? type) => type?.Trim().ToLowerInvariant() switch
    {
        "dni" => IdentityDocumentType.Dni,
        "foreignercard" or "ce" or "carne" or "carné" or "carne de extranjeria" or "carné de extranjería"
            => IdentityDocumentType.ForeignerCard,
        _ => throw new UnknownIdentityDocumentTypeException(type)
    };

    public override string ToString() => $"{Type} {Number}";

    [GeneratedRegex(@"[\s\-.]")]
    private static partial Regex Separators();

    [GeneratedRegex(@"^\d{8}$")]
    private static partial Regex Dni();

    [GeneratedRegex(@"^[A-Z0-9]{9,12}$")]
    private static partial Regex ForeignerCard();
}

public class InvalidIdentityDocumentException(IdentityDocumentType type, string? value)
    : InvalidDomainDataException(type == IdentityDocumentType.Dni
        ? $"El DNI '{value}' no es válido: debe tener exactamente 8 dígitos."
        : $"El carné de extranjería '{value}' no es válido: debe tener entre 9 y 12 letras o dígitos.")
{
    public override string Code => "invalid-identity-document";
}

public class UnknownIdentityDocumentTypeException(string? value)
    : InvalidDomainDataException(
        $"El tipo de documento '{value}' no se admite. Los tipos admitidos son DNI y carné de extranjería.")
{
    public override string Code => "unknown-identity-document-type";
}
