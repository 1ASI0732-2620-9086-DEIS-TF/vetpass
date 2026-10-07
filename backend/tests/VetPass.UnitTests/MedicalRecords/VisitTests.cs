using VetPass.API.MedicalRecords.Domain.Model.Aggregates;
using VetPass.API.MedicalRecords.Domain.Model.Entities;
using VetPass.API.MedicalRecords.Domain.Model.ValueObjects;

namespace VetPass.UnitTests.MedicalRecords;

/// <summary>Visits and prescriptions (US13, US15).</summary>
public class VisitTests
{
    private static readonly DateOnly Today = new(2026, 10, 7);

    private static Visit NewVisit(string reason = "Control", string diagnosis = "Sano", DateOnly? date = null) =>
        new(Guid.NewGuid(), Guid.NewGuid(), date ?? Today, reason, "Sin hallazgos", diagnosis, null, 6.8m, Today);

    [Fact]
    public void Constructor_RequiresReasonAndDiagnosis()
    {
        Should.Throw<RequiredVisitFieldException>(() => NewVisit(reason: ""));
        Should.Throw<RequiredVisitFieldException>(() => NewVisit(diagnosis: " "));
    }

    [Fact]
    public void Constructor_FutureDate_IsRejected() =>
        Should.Throw<FutureVisitDateException>(() => NewVisit(date: Today.AddDays(1)));

    [Fact]
    public void IssuePrescription_KeepsItsItems()
    {
        var visit = NewVisit();

        visit.IssuePrescription([new PrescriptionItem("Cefalexina 250 mg", "Media tableta cada 12 horas", "7 días")]);

        visit.HasPrescription().ShouldBeTrue();
        visit.Prescription!.Items.Count.ShouldBe(1);
    }

    [Fact]
    public void IssuePrescription_WithoutItems_IsRejected() =>
        Should.Throw<EmptyPrescriptionException>(() => NewVisit().IssuePrescription([]));

    [Fact]
    public void IssuePrescription_Twice_IsRejected()
    {
        var visit = NewVisit();
        visit.IssuePrescription([new PrescriptionItem("A", "B", "C")]);

        Should.Throw<PrescriptionAlreadyIssuedException>(() =>
            visit.IssuePrescription([new PrescriptionItem("A", "B", "C")]));
    }

    [Fact]
    public void PrescriptionItem_RequiresEveryField() =>
        Should.Throw<RequiredPrescriptionFieldException>(() => new PrescriptionItem("Cefalexina", "", "7 días"));
}
