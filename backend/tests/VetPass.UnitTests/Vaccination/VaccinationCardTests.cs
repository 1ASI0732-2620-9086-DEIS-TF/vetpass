using VetPass.API.Shared.Domain.Exceptions;
using VetPass.API.Vaccination.Domain.Model.Aggregates;
using VetPass.API.Vaccination.Domain.Model.Entities;
using VetPass.API.Vaccination.Domain.Model.ValueObjects;
using VetPass.API.Vaccination.Domain.Services;
using static VetPass.UnitTests.Vaccination.CanineSchedule;

namespace VetPass.UnitTests.Vaccination;

/// <summary>Rules of the vaccination card (US09, US10, US11).</summary>
public class VaccinationCardTests
{
    // Rocky, the case of section 4.6: born on 5 August 2026.
    private static readonly DateOnly Birth = new(2026, 8, 5);
    private static readonly Guid Vet = Guid.NewGuid();

    private static DateOnly Weeks(int weeks) => Birth.AddDays(weeks * 7);

    private static VaccinationCard NewCard(DateOnly registeredOn, DateOnly? birth = null) =>
        VaccinationCard.GenerateFrom(Guid.NewGuid(), Create(), birth ?? Birth, registeredOn);

    private static Dose DoseOf(VaccinationCard card, Guid vaccine, int sequence) =>
        card.Doses.Single(d => d.VaccineId == vaccine && d.SequenceNumber == sequence);

    private static void Apply(VaccinationCard card, Guid vaccine, int sequence, DateOnly date, DateOnly? today = null) =>
        card.RegisterDose(DoseOf(card, vaccine, sequence).Id, date, new BatchCode("A-1"), Vet, today ?? date);

    [Fact]
    public void GenerateFrom_Newborn_ExpectsEachDoseAtItsMinimumAge()
    {
        var card = NewCard(registeredOn: Birth);

        card.Doses.Count.ShouldBe(6);
        card.Doses.ShouldAllBe(d => d.Status == DoseStatus.Pending);
        DoseOf(card, Multivalent, 1).ExpectedDate.ShouldBe(Weeks(6));
        DoseOf(card, Multivalent, 2).ExpectedDate.ShouldBe(Weeks(9));
        DoseOf(card, Multivalent, 3).ExpectedDate.ShouldBe(Weeks(12));
        DoseOf(card, Rabies, 1).ExpectedDate.ShouldBe(Weeks(12));
    }

    [Fact]
    public void GenerateFrom_AdultPet_PlansTheScheduleFromToday()
    {
        var today = new DateOnly(2026, 10, 7);
        var card = NewCard(today, birth: today.AddYears(-4));

        DoseOf(card, Multivalent, 1).ExpectedDate.ShouldBe(today);
        DoseOf(card, Multivalent, 2).ExpectedDate.ShouldBe(today.AddDays(21));
        DoseOf(card, Multivalent, 3).ExpectedDate.ShouldBe(today.AddDays(42));
        DoseOf(card, Rabies, 1).ExpectedDate.ShouldBe(today);
        card.Doses.ShouldAllBe(d => d.ExpectedDate >= today);
    }

    [Fact]
    public void RegisterDose_ValidDose_IsAppliedAndPlansTheNextOne()
    {
        var card = NewCard(Weeks(6));

        Apply(card, Multivalent, 1, Weeks(6));

        var first = DoseOf(card, Multivalent, 1);
        first.Status.ShouldBe(DoseStatus.Applied);
        first.ApplicationDate.ShouldBe(Weeks(6));
        first.BatchCode!.Value.ShouldBe("A-1");
        DoseOf(card, Multivalent, 2).ExpectedDate.ShouldBe(Weeks(9));
    }

    [Fact]
    public void RegisterDose_AlreadyApplied_IsRejected()
    {
        var card = NewCard(Weeks(6));
        Apply(card, Multivalent, 1, Weeks(6));

        Should.Throw<DoseAlreadyAppliedException>(() => Apply(card, Multivalent, 1, Weeks(6)));
    }

    [Fact]
    public void RegisterDose_FutureDate_IsRejected()
    {
        var card = NewCard(Weeks(6));

        Should.Throw<FutureApplicationDateException>(() =>
            Apply(card, Multivalent, 1, Weeks(7), today: Weeks(6)));
    }

    [Fact]
    public void RegisterDose_BeforeThePreviousDoseOfTheSameVaccine_IsRejected()
    {
        var card = NewCard(Weeks(9));

        var error = Should.Throw<DoseOutOfOrderException>(() => Apply(card, Multivalent, 2, Weeks(9)));

        error.PendingSequenceNumber.ShouldBe(1);
        error.Code.ShouldBe("dose-out-of-order");
    }

    [Fact]
    public void RegisterDose_DifferentVaccinesOnTheSameDay_AreAccepted()
    {
        var card = NewCard(Weeks(6));
        Apply(card, Multivalent, 1, Weeks(6));
        Apply(card, Multivalent, 2, Weeks(9));

        Apply(card, Multivalent, 3, Weeks(12));
        Apply(card, Rabies, 1, Weeks(12));

        DoseOf(card, Multivalent, 3).IsApplied().ShouldBeTrue();
        DoseOf(card, Rabies, 1).IsApplied().ShouldBeTrue();
    }

    [Fact]
    public void RegisterDose_BelowTheMinimumAge_IsRejectedWithTheEarliestDate()
    {
        var card = NewCard(Weeks(9));

        var error = Should.Throw<MinimumAgeNotReachedException>(() => Apply(card, Rabies, 1, Weeks(9)));

        error.AgeInWeeks.ShouldBe(9);
        error.RequiredWeeks.ShouldBe(12);
        error.EarliestAdmissibleDate.ShouldBe(Weeks(12));
    }

    [Fact]
    public void RegisterDose_BeforeTheMinimumInterval_IsRejectedWithTheEarliestDate()
    {
        var card = NewCard(Weeks(8));
        Apply(card, Multivalent, 1, Weeks(8));

        var error = Should.Throw<MinimumIntervalNotMetException>(() => Apply(card, Multivalent, 2, Weeks(10)));

        error.RequiredWeeks.ShouldBe(3);
        error.EarliestAdmissibleDate.ShouldBe(Weeks(11));
    }

    [Fact]
    public void RegisterDose_HistoricalDose_ReplansOnlyItsOwnSeriesFromToday()
    {
        var today = new DateOnly(2026, 10, 7);
        var birth = today.AddYears(-4);
        var card = NewCard(today, birth);
        var rabiesBefore = DoseOf(card, Rabies, 1).ExpectedDate;

        card.RegisterDose(DoseOf(card, Multivalent, 1).Id, birth.AddDays(7 * 7), new BatchCode("H-1"), Vet, today);

        DoseOf(card, Multivalent, 2).ExpectedDate.ShouldBe(today);
        DoseOf(card, Multivalent, 3).ExpectedDate.ShouldBe(today.AddDays(21));
        DoseOf(card, Rabies, 1).ExpectedDate.ShouldBe(rabiesBefore);
    }

    [Fact]
    public void RegisterDose_UnknownDose_IsNotFound()
    {
        var card = NewCard(Weeks(6));

        Should.Throw<ResourceNotFoundException>(() =>
            card.RegisterDose(Guid.NewGuid(), Weeks(6), new BatchCode("A-1"), Vet, Weeks(6)));
    }

    [Fact]
    public void GetStatus_ReflectsTheDosesDueAtTheDate()
    {
        var card = NewCard(Weeks(6));

        card.GetStatus(Weeks(5)).ShouldBe(CardStatus.UpToDate);
        card.GetStatus(Weeks(6)).ShouldBe(CardStatus.Pending);
        card.GetStatus(Weeks(7)).ShouldBe(CardStatus.Overdue);

        Apply(card, Multivalent, 1, Weeks(6));
        card.GetStatus(Weeks(7)).ShouldBe(CardStatus.UpToDate);
    }

    [Fact]
    public void IsNextInSequence_OnlyTheDoseItsSeriesIsWaitingFor()
    {
        var card = NewCard(Weeks(6));

        card.IsNextInSequence(DoseOf(card, Multivalent, 1)).ShouldBeTrue();
        card.IsNextInSequence(DoseOf(card, Multivalent, 2)).ShouldBeFalse();
        card.IsNextInSequence(DoseOf(card, Rabies, 1)).ShouldBeTrue();
        card.EarliestAdmissibleDate(DoseOf(card, Multivalent, 2)).ShouldBeNull();

        Apply(card, Multivalent, 1, Weeks(7));

        card.IsNextInSequence(DoseOf(card, Multivalent, 2)).ShouldBeTrue();
        card.EarliestAdmissibleDate(DoseOf(card, Multivalent, 2)).ShouldBe(Weeks(10));
    }

    [Fact]
    public void NextExpectedDose_IsThePendingDoseWithTheEarliestDate()
    {
        var card = NewCard(Weeks(6));
        Apply(card, Multivalent, 1, Weeks(6));

        var next = card.NextExpectedDose()!;

        next.VaccineId.ShouldBe(Multivalent);
        next.SequenceNumber.ShouldBe(2);
    }
}
