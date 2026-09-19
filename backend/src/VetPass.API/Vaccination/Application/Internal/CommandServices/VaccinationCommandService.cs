using VetPass.API.Shared.Domain.Exceptions;
using VetPass.API.Shared.Domain.Services;
using VetPass.API.Vaccination.Domain.Model.Aggregates;
using VetPass.API.Vaccination.Domain.Model.Commands;
using VetPass.API.Vaccination.Domain.Repositories;
using VetPass.API.Vaccination.Domain.Services;

namespace VetPass.API.Vaccination.Application.Internal.CommandServices;

/// <summary>
/// Orchestrates the generation of a card and the registration of a dose. It
/// coordinates: every rule it applies belongs to the aggregate, not to this
/// service.
/// </summary>
public class VaccinationCommandService(
    IVaccinationCardRepository cards,
    IVaccinationScheduleProvider scheduleProvider,
    IClinicClock clock,
    IUnitOfWork unitOfWork)
{
    /// <summary>
    /// Generates the card of a pet. It does not close the transaction: the
    /// registration of the pet that originates it does, so that a pet without
    /// its card cannot exist (US09-E1).
    /// </summary>
    public async Task<VaccinationCard> GenerateForAsync(GenerateVaccinationCardCommand command,
        CancellationToken cancellationToken = default)
    {
        if (await cards.ExistsForPetAsync(command.PetId, cancellationToken))
            throw new CardAlreadyExistsException(command.PetId);

        var schedule = await scheduleProvider.GetForAsync(command.Species, cancellationToken);
        var card = VaccinationCard.GenerateFrom(command.PetId, schedule, command.BirthDate);

        await cards.AddAsync(card, cancellationToken);
        return card;
    }

    /// <summary>
    /// Records an applied dose (US10). Every check lives inside the aggregate;
    /// what is decided here is only which card is loaded and which date counts
    /// as today.
    /// </summary>
    public async Task<VaccinationCard> RegisterDoseAsync(RegisterDoseCommand command,
        CancellationToken cancellationToken = default)
    {
        var card = await cards.FindByPetIdAsync(command.PetId, cancellationToken)
                   ?? throw new ResourceNotFoundException("una cartilla para la mascota", command.PetId);

        card.RegisterDose(command.DoseId, command.ApplicationDate, command.BatchCode,
            command.VeterinarianId, clock.Today);

        await unitOfWork.CompleteAsync(cancellationToken);
        return card;
    }
}

public class CardAlreadyExistsException(Guid petId)
    : InvalidDomainDataException($"La mascota {petId} ya tiene una cartilla de vacunación emitida.")
{
    public override string Code => "card-already-exists";
}
