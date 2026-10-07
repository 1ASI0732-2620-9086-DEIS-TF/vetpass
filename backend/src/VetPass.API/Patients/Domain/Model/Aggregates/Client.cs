using VetPass.API.Patients.Domain.Model.ValueObjects;
using VetPass.API.Shared.Domain.Exceptions;

namespace VetPass.API.Patients.Domain.Model.Aggregates;

/// <summary>
/// A client of the clinic: the person responsible for one or more pets and the
/// holder of the mobile application account.
/// </summary>
public class Client
{
    public Guid Id { get; private set; }
    public Guid ClinicId { get; private set; }
    public string FullName { get; private set; } = null!;

    /// <summary>
    /// Contact number in E.164 form. The client receives it already validated,
    /// as a <see cref="ValueObjects.PhoneNumber"/>, so that no client can exist
    /// with a number outside Peru; it is kept as text so that rows recorded
    /// before the rule existed remain readable.
    /// </summary>
    public string PhoneNumber { get; private set; } = null!;
    public string? Email { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Required by Entity Framework Core.
    private Client() { }

    public Client(Guid clinicId, string fullName, PhoneNumber phoneNumber, string? email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new RequiredClientFieldException("nombres y apellidos");

        Id = Guid.NewGuid();
        ClinicId = clinicId;
        FullName = fullName.Trim();
        PhoneNumber = phoneNumber.Value;
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateContactInfo(PhoneNumber phoneNumber, string? email)
    {
        PhoneNumber = phoneNumber.Value;
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
    }
}

public class RequiredClientFieldException(string field)
    : InvalidDomainDataException($"El campo '{field}' del cliente es obligatorio.")
{
    public override string Code => "required-client-field";
}
