using VetPass.API.Patients.Domain.Model.ValueObjects;

namespace VetPass.UnitTests.Patients;

/// <summary>DNI or foreigner card that identifies a client (US06-E4).</summary>
public class IdentityDocumentTests
{
    [Fact]
    public void Of_Dni_KeepsEightDigits() =>
        IdentityDocument.Of(IdentityDocumentType.Dni, "4587 9123").Number.ShouldBe("45879123");

    [Fact]
    public void Of_ForeignerCard_IsUpperCasedWithoutSeparators() =>
        IdentityDocument.Of(IdentityDocumentType.ForeignerCard, "cx-0012345").Number.ShouldBe("CX0012345");

    [Theory]
    [InlineData(IdentityDocumentType.Dni, "1234567")]
    [InlineData(IdentityDocumentType.Dni, "123456789")]
    [InlineData(IdentityDocumentType.Dni, "1234567A")]
    [InlineData(IdentityDocumentType.ForeignerCard, "12345678")]
    [InlineData(IdentityDocumentType.ForeignerCard, "1234567890123")]
    public void Of_InvalidNumber_IsRejected(IdentityDocumentType type, string number) =>
        Should.Throw<InvalidIdentityDocumentException>(() => IdentityDocument.Of(type, number));

    [Theory]
    [InlineData("dni", IdentityDocumentType.Dni)]
    [InlineData("CE", IdentityDocumentType.ForeignerCard)]
    [InlineData("ForeignerCard", IdentityDocumentType.ForeignerCard)]
    public void ParseType_AcceptsTheClinicAbbreviations(string written, IdentityDocumentType expected) =>
        IdentityDocument.ParseType(written).ShouldBe(expected);

    [Fact]
    public void ParseType_Unknown_IsRejected() =>
        Should.Throw<UnknownIdentityDocumentTypeException>(() => IdentityDocument.ParseType("Pasaporte"));

    [Fact]
    public void TwoWaysOfWritingTheSameDocument_AreEqual() =>
        IdentityDocument.Parse("DNI", "45-879-123").ShouldBe(IdentityDocument.Parse("dni", "45879123"));
}
