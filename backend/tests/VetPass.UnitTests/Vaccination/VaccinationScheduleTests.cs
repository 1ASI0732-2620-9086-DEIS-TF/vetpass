using VetPass.API.Patients.Domain.Model.ValueObjects;
using VetPass.API.Vaccination.Domain.Model.ValueObjects;
using VetPass.API.Vaccination.Domain.Services;

namespace VetPass.UnitTests.Vaccination;

public class VaccinationScheduleTests
{
    [Fact]
    public void Constructor_WithoutItems_IsRejected() =>
        Should.Throw<EmptyScheduleException>(() => new VaccinationSchedule(Species.Canine, []));

    [Fact]
    public void ExpectedDateFor_IsBirthPlusTheMinimumAge()
    {
        var schedule = CanineSchedule.Create();
        var birth = new DateOnly(2026, 8, 5);

        schedule.ExpectedDateFor(schedule.Items[0], birth).ShouldBe(birth.AddDays(42));
    }

    [Theory]
    [InlineData("a-4471", "A-4471")]
    [InlineData("  R-1180 ", "R-1180")]
    public void BatchCode_IsTrimmedAndUpperCased(string written, string expected) =>
        new BatchCode(written).Value.ShouldBe(expected);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BatchCode_Empty_IsRejected(string? written) =>
        Should.Throw<InvalidBatchCodeException>(() => new BatchCode(written));
}
