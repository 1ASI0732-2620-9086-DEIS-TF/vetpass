using VetPass.API.Patients.Domain.Model.ValueObjects;

namespace VetPass.UnitTests.Patients;

/// <summary>Peruvian phone numbers only (US06-E3), stored in E.164.</summary>
public class PhoneNumberTests
{
    [Theory]
    [InlineData("987654321", "+51987654321")]
    [InlineData("987 654 321", "+51987654321")]
    [InlineData("+51 987 654 321", "+51987654321")]
    [InlineData("0051987654321", "+51987654321")]
    [InlineData("51987654321", "+51987654321")]
    [InlineData("(01) 234-5678", "+5112345678")]
    [InlineData("044 123456", "+5144123456")]
    public void Parse_PeruvianNumber_IsStoredInE164(string written, string expected) =>
        PhoneNumber.Parse(written).Value.ShouldBe(expected);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("887654321")]
    [InlineData("+1 202 555 0100")]
    [InlineData("98765432a")]
    public void Parse_NotAPeruvianNumber_IsRejected(string? written) =>
        Should.Throw<InvalidPhoneNumberException>(() => PhoneNumber.Parse(written));

    [Fact]
    public void IsMobile_DistinguishesMobilesFromLandlines()
    {
        PhoneNumber.Parse("987654321").IsMobile.ShouldBeTrue();
        PhoneNumber.Parse("(01) 234-5678").IsMobile.ShouldBeFalse();
    }
}
