using VetPass.API.Patients.Domain.Model.ValueObjects;
using VetPass.API.Vaccination.Domain.Model.ValueObjects;

namespace VetPass.API.Vaccination.Domain.Model.Commands;

/// <summary>
/// Generates the card of a pet just registered. It is not requested by the user:
/// it is a consequence of the registration (US09-E1).
/// </summary>
public record GenerateVaccinationCardCommand(Guid PetId, Species Species, DateOnly BirthDate);

public record RegisterDoseCommand(Guid PetId, Guid DoseId, DateOnly ApplicationDate,
    BatchCode BatchCode, Guid VeterinarianId);
