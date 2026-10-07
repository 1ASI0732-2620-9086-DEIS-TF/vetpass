using VetPass.API.Vaccination.Application.Internal.QueryServices;
using VetPass.API.Vaccination.Domain.Model.Entities;
using VetPass.API.Vaccination.Interfaces.REST.Resources;

namespace VetPass.API.Vaccination.Interfaces.REST.Transform;

public static class VaccinationCardResourceFromEntityAssembler
{
    /// <summary>
    /// A booster is recognised by its minimum age: a dose demanded from the
    /// first year of life onwards no longer belongs to the initial series.
    /// </summary>
    private const int BoosterMinimumAgeInWeeks = 52;

    public static VaccinationCardResource ToResource(VaccinationCardView view)
    {
        var card = view.Card;

        // The doses of the card read in chronological order, from the first to
        // the last, which is how the schedule is read (section 4.2.1).
        var doses = card.Doses
            .OrderBy(dose => dose.ExpectedDate)
            .ThenBy(dose => dose.SequenceNumber)
            .Select(dose => ToResource(dose, view))
            .ToList();

        return new VaccinationCardResource(
            card.Id,
            card.PetId,
            card.Species.ToString(),
            card.PetBirthDate,
            view.Status.ToString(),
            view.NextDose is null ? null : ToResource(view.NextDose, view),
            doses);
    }

    private static DoseResource ToResource(Dose dose, VaccinationCardView view)
    {
        var name = view.Vaccines.TryGetValue(dose.VaccineId, out var vaccine)
            ? vaccine.Name
            : "Vacuna";

        return new DoseResource(
            dose.Id,
            dose.VaccineId,
            name,
            // La etiqueta viaja armada para clientes que no compongan texto,
            // y el indicador de refuerzo para los que sí lo hagan en su idioma.
            $"{name} · {LabelFor(dose)}",
            dose.MinimumAgeInWeeks >= BoosterMinimumAgeInWeeks,
            dose.SequenceNumber,
            dose.ExpectedDate,
            dose.ApplicationDate,
            dose.BatchCode?.Value,
            dose.VeterinarianId,
            dose.Status.ToString(),
            dose.IsOverdue(view.Today),
            view.Card.IsNextInSequence(dose),
            view.Card.EarliestAdmissibleDate(dose));
    }

    private static string LabelFor(Dose dose) => dose.MinimumAgeInWeeks >= BoosterMinimumAgeInWeeks
        ? "refuerzo anual"
        : $"{dose.SequenceNumber}.ª dosis";
}
