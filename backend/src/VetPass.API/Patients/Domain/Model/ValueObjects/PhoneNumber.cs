using System.Text.RegularExpressions;
using VetPass.API.Shared.Domain.Exceptions;

namespace VetPass.API.Patients.Domain.Model.ValueObjects;

/// <summary>
/// Contact number of a client of the clinic. Only Peruvian numbers are admitted,
/// because the product serves clinics of Lima and their clients, and the number
/// exists to reach the owner of the pet.
///
/// It accepts the forms in which a number is usually written —with or without
/// the country code, with spaces, dashes or parentheses, with the trunk prefix 0
/// of a landline— and keeps it in a single canonical form, E.164: +51 followed
/// by the national number. Two ways of writing the same number are therefore
/// the same value.
///
/// Admitted national numbers, according to the Peruvian numbering plan:
/// <list type="bullet">
///   <item>Mobile: nine digits beginning with 9.</item>
///   <item>Landline in Lima and Callao: area code 1 and seven digits.</item>
///   <item>Landline in the provinces: two-digit area code and six digits.</item>
/// </list>
/// </summary>
public sealed partial record PhoneNumber
{
    private const string CountryCode = "51";

    /// <summary>Canonical form, E.164: +51 followed by the national number.</summary>
    public string Value { get; }

    public bool IsMobile => Value.StartsWith("+519", StringComparison.Ordinal);

    private PhoneNumber(string value) => Value = value;

    public static PhoneNumber Parse(string? input)
    {
        var written = input?.Trim();
        if (string.IsNullOrEmpty(written) || !AdmittedCharacters().IsMatch(written))
            throw new InvalidPhoneNumberException(input);

        var digits = NonDigits().Replace(written, string.Empty);
        var national = NationalPart(written, digits)
                       ?? throw new InvalidPhoneNumberException(input);

        // The trunk prefix 0 belongs to domestic dialling of landlines (01 for
        // Lima, 044 for Trujillo) and is not part of the number.
        if (national.Length == 9 && national[0] == '0')
            national = national[1..];

        if (!Mobile().IsMatch(national) && !LimaLandline().IsMatch(national) &&
            !ProvinceLandline().IsMatch(national))
            throw new InvalidPhoneNumberException(input);

        return new PhoneNumber($"+{CountryCode}{national}");
    }

    /// <summary>
    /// The national number, once the country code is removed if it was written.
    /// A number written with a plus sign must carry the Peruvian code: any other
    /// one belongs to a foreign line.
    /// </summary>
    private static string? NationalPart(string written, string digits)
    {
        if (written.StartsWith('+'))
            return digits.StartsWith(CountryCode, StringComparison.Ordinal) ? digits[2..] : null;

        if (digits.StartsWith("00", StringComparison.Ordinal))
            return digits.StartsWith("00" + CountryCode, StringComparison.Ordinal) ? digits[4..] : null;

        // Without a plus sign, the code can only be told apart by length: a
        // national number has eight or nine digits, so ten or eleven digits
        // beginning with 51 carry the country code.
        if (digits.Length is 10 or 11 && digits.StartsWith(CountryCode, StringComparison.Ordinal))
            return digits[2..];

        return digits;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^\+?[\d\s().\-]+$")]
    private static partial Regex AdmittedCharacters();

    [GeneratedRegex(@"\D")]
    private static partial Regex NonDigits();

    [GeneratedRegex(@"^9\d{8}$")]
    private static partial Regex Mobile();

    [GeneratedRegex(@"^1\d{7}$")]
    private static partial Regex LimaLandline();

    /// <summary>
    /// Area codes of the provinces: 41 to 44, 51 to 54 and 56, 61 to 67, 72 to
    /// 74 and 76, and 82 to 84.
    /// </summary>
    [GeneratedRegex(@"^(4[1-4]|5[1-46]|6[1-7]|7[2-46]|8[2-4])\d{6}$")]
    private static partial Regex ProvinceLandline();
}

public class InvalidPhoneNumberException(string? value)
    : InvalidDomainDataException(
        $"El teléfono '{value}' no es un número peruano válido. Se admite un celular de 9 dígitos " +
        "que empieza con 9, o un teléfono fijo con su código de área (01 para Lima).")
{
    public override string Code => "invalid-phone-number";
}
