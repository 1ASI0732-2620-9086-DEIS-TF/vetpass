using System.ComponentModel.DataAnnotations;

namespace VetPass.API.Vaccination.Interfaces.REST.Resources;

public record RegisterDoseResource(
    [Required] DateOnly ApplicationDate,
    [Required] string BatchCode,
    Guid? VeterinarianId);

public record DoseResource(
    Guid Id,
    Guid VaccineId,
    string VaccineName,
    string Label,
    bool IsBooster,
    int SequenceNumber,
    DateOnly ExpectedDate,
    DateOnly? ApplicationDate,
    string? BatchCode,
    Guid? VeterinarianId,
    string Status,
    bool IsOverdue);

public record VaccinationCardResource(
    Guid Id,
    Guid PetId,
    string Species,
    DateOnly PetBirthDate,
    string Status,
    DoseResource? NextDose,
    IEnumerable<DoseResource> Doses);
