using VetPass.API.IAM.Domain.Model.Aggregates;
using VetPass.API.IAM.Domain.Model.ValueObjects;
using VetPass.API.IAM.Domain.Services;

namespace VetPass.UnitTests.IAM;

/// <summary>Roles, temporary passwords and password policy (US04, US05, US17).</summary>
public class UserProfileTests
{
    [Fact]
    public void ClinicStaff_CanRegisterClinicalData() =>
        new UserProfile(Guid.NewGuid(), "andrea@vet.pe", "Andrea", Role.ClinicStaff, Guid.NewGuid(), null)
            .CanRegisterClinicalData().ShouldBeTrue();

    [Fact]
    public void PetOwner_CannotRegisterClinicalData() =>
        new UserProfile(Guid.NewGuid(), "valeria@correo.com", "Valeria", Role.PetOwner, Guid.NewGuid(), Guid.NewGuid())
            .CanRegisterClinicalData().ShouldBeFalse();

    [Fact]
    public void Profile_WithoutItsClinicOrClient_IsRejected()
    {
        Should.Throw<IncompleteUserProfileException>(() =>
            new UserProfile(Guid.NewGuid(), "a@vet.pe", "A", Role.ClinicStaff, null, null));
        Should.Throw<IncompleteUserProfileException>(() =>
            new UserProfile(Guid.NewGuid(), "v@correo.com", "V", Role.PetOwner, Guid.NewGuid(), null));
    }

    [Fact]
    public void TemporaryPassword_IsMarkedUntilTheOwnerChangesIt()
    {
        var profile = new UserProfile(Guid.NewGuid(), "v@correo.com", "V", Role.PetOwner, Guid.NewGuid(), Guid.NewGuid());

        profile.MarkTemporaryPasswordIssued();
        profile.RequiresPasswordChange.ShouldBeTrue();

        profile.MarkPasswordChanged();
        profile.RequiresPasswordChange.ShouldBeFalse();
    }

    [Theory]
    [InlineData("ClinicStaff", Role.ClinicStaff)]
    [InlineData("pet_owner", Role.PetOwner)]
    public void ToRole_ReadsTheClaim(string claim, Role expected) => claim.ToRole().ShouldBe(expected);

    [Fact]
    public void ToRole_Unknown_IsRejected() => Should.Throw<UnknownRoleException>(() => "admin".ToRole());

    [Theory]
    [InlineData("Perrito2026")]
    [InlineData("abc12345")]
    public void PasswordPolicy_AcceptsLettersAndDigits(string password) =>
        Should.NotThrow(() => PasswordPolicy.Ensure(password, "Anterior2025"));

    [Theory]
    [InlineData("corta1")]
    [InlineData("soloLetras")]
    [InlineData("12345678")]
    [InlineData("")]
    public void PasswordPolicy_RejectsWeakPasswords(string password) =>
        Should.Throw<WeakPasswordException>(() => PasswordPolicy.Ensure(password, "Anterior2025"));

    [Fact]
    public void PasswordPolicy_RejectsTheSamePassword() =>
        Should.Throw<PasswordNotChangedException>(() => PasswordPolicy.Ensure("Perrito2026", "Perrito2026"));
}
