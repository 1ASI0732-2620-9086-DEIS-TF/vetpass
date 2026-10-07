using VetPass.API.Patients.Domain.Model.Aggregates;
using VetPass.API.Patients.Domain.Repositories;
using VetPass.API.Patients.Interfaces.REST.Resources;
using VetPass.API.Vaccination.Domain.Model.ValueObjects;

namespace VetPass.API.Patients.Interfaces.REST.Transform;

public static class ClientResourceFromEntityAssembler
{
    public static ClientResource ToResource(Client client, bool hasAccount = false) =>
        new(client.Id, client.FullName, client.DocumentType.ToString(), client.DocumentNumber,
            client.PhoneNumber, client.Email, hasAccount);
}

public static class PetResourceFromEntityAssembler
{
    public static PetResource ToResource(Patient patient, DateOnly today) => new(
        patient.Pet.Id,
        patient.Pet.Name,
        patient.Pet.Species.ToString(),
        patient.Pet.Breed,
        patient.Pet.Sex.ToString(),
        patient.Pet.BirthDate,
        patient.Pet.AgeInWeeks(today),
        ClientResourceFromEntityAssembler.ToResource(patient.Owner));
}

public static class PatientResourceFromEntityAssembler
{
    public static PatientResource ToResource(Patient patient, CardStatus? status, DateOnly today) => new(
        patient.Pet.Id,
        patient.Pet.Name,
        patient.Pet.Species.ToString(),
        patient.Pet.Breed,
        patient.Pet.BirthDate,
        patient.Pet.AgeInWeeks(today),
        ClientResourceFromEntityAssembler.ToResource(patient.Owner),
        status?.ToString());
}
