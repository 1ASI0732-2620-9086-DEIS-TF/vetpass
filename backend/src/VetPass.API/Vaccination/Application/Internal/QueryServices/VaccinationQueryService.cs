using VetPass.API.Shared.Domain.Exceptions;
using VetPass.API.Shared.Domain.Services;
using VetPass.API.Vaccination.Domain.Model.Aggregates;
using VetPass.API.Vaccination.Domain.Model.Entities;
using VetPass.API.Vaccination.Domain.Model.ValueObjects;
using VetPass.API.Vaccination.Domain.Repositories;

namespace VetPass.API.Vaccination.Application.Internal.QueryServices;

/// <summary>
/// A card resolved for consultation: the card itself, the status calculated for
/// today, the next expected dose and the names of the vaccines involved.
/// </summary>
public record VaccinationCardView(
    VaccinationCard Card,
    CardStatus Status,
    Dose? NextDose,
    IReadOnlyDictionary<Guid, Vaccine> Vaccines,
    DateOnly Today);

public class VaccinationQueryService(
    IVaccinationCardRepository cards,
    IVaccineCatalogRepository catalog,
    IClinicClock clock)
{
    public async Task<VaccinationCardView> GetByPetIdAsync(Guid petId,
        CancellationToken cancellationToken = default)
    {
        var card = await cards.FindByPetIdAsync(petId, cancellationToken)
                   ?? throw new ResourceNotFoundException("una cartilla para la mascota", petId);

        var vaccines = (await catalog.ListVaccinesAsync(cancellationToken))
            .ToDictionary(vaccine => vaccine.Id);

        var today = clock.Today;
        return new VaccinationCardView(card, card.GetStatus(today), card.NextExpectedDose(), vaccines, today);
    }

    /// <summary>
    /// Status of several cards at once, which is what the listing of patients
    /// needs in order to show one chip per row without asking for each card
    /// separately.
    /// </summary>
    public async Task<IReadOnlyDictionary<Guid, CardStatus>> GetStatusesAsync(IEnumerable<Guid> petIds,
        CancellationToken cancellationToken = default)
    {
        var today = clock.Today;
        var found = await cards.ListByPetIdsAsync(petIds, cancellationToken);

        return found.ToDictionary(card => card.PetId, card => card.GetStatus(today));
    }
}
