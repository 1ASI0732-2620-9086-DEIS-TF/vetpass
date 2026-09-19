using System.ComponentModel.DataAnnotations;

namespace VetPass.API.Patients.Interfaces.REST.Resources;

public record CreateClientResource(
    [Required] string FullName,
    [Required] string PhoneNumber,
    [EmailAddress] string? Email);

public record CreatePetResource(
    [Required] Guid ClientId,
    [Required] string Name,
    [Required] string Species,
    string? Breed,
    [Required] string Sex,
    [Required] DateOnly BirthDate);

public record ClientResource(Guid Id, string FullName, string PhoneNumber, string? Email);

public record PetResource(
    Guid Id,
    string Name,
    string Species,
    string? Breed,
    string Sex,
    DateOnly BirthDate,
    int AgeInWeeks,
    ClientResource Owner);

/// <summary>
/// A row of the patient listing: the pet, its owner and the status of its card,
/// which is the information the interface shows without opening the record
/// (section 4.2.4).
/// </summary>
public record PatientResource(
    Guid Id,
    string Name,
    string Species,
    string? Breed,
    DateOnly BirthDate,
    int AgeInWeeks,
    ClientResource Owner,
    string? CardStatus);
