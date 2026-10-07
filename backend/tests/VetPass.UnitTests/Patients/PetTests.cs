using VetPass.API.Patients.Domain.Model.Aggregates;
using VetPass.API.Patients.Domain.Model.ValueObjects;

namespace VetPass.UnitTests.Patients;

/// <summary>Registration of a pet (US07).</summary>
public class PetTests
{
    private static readonly DateOnly Today = new(2026, 10, 7);

    private static Pet NewPet(Species species, DateOnly birth, string name = "Rocky") =>
        new(Guid.NewGuid(), name, species, "Beagle", Sex.Male, birth, Today);

    [Fact]
    public void Constructor_ValidData_RegistersThePet()
    {
        var pet = NewPet(Species.Canine, new DateOnly(2026, 8, 5));

        pet.Name.ShouldBe("Rocky");
        pet.AgeInWeeks(Today).ShouldBe(9);
    }

    [Fact]
    public void Constructor_WithoutName_IsRejected() =>
        Should.Throw<RequiredPetFieldException>(() => NewPet(Species.Canine, Today, name: " "));

    [Fact]
    public void Constructor_FutureBirthDate_IsRejected() =>
        Should.Throw<FutureBirthDateException>(() => NewPet(Species.Canine, Today.AddDays(1)));

    [Theory]
    [InlineData(Species.Canine, 26)]
    [InlineData(Species.Feline, 31)]
    public void Constructor_ImplausibleAge_IsRejected(Species species, int years)
    {
        var error = Should.Throw<ImplausibleBirthDateException>(() => NewPet(species, Today.AddYears(-years)));

        error.Code.ShouldBe("implausible-birth-date");
    }

    [Theory]
    [InlineData(Species.Canine, 25)]
    [InlineData(Species.Feline, 30)]
    public void Constructor_AtTheMaximumAge_IsAccepted(Species species, int years) =>
        NewPet(species, Today.AddYears(-years)).Species.ShouldBe(species);

    [Fact]
    public void ToSpecies_UnsupportedSpecies_IsRejected() =>
        Should.Throw<UnsupportedSpeciesException>(() => "Rabbit".ToSpecies());
}
