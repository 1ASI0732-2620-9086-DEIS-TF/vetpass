using VetPass.API.Patients.Domain.Model.ValueObjects;

namespace VetPass.API.Patients.Domain.Model.Commands;

public record CreateClientCommand(Guid ClinicId, string FullName, IdentityDocument Document,
    PhoneNumber PhoneNumber, string? Email);

public record CreatePetCommand(Guid ClientId, string Name, Species Species, string? Breed,
    Sex Sex, DateOnly BirthDate);
