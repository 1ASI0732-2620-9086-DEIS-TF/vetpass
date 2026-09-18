using VetPass.API.IAM.Domain.Model.ValueObjects;
using VetPass.API.Shared.Domain.Exceptions;

namespace VetPass.API.IAM.Domain.Model.Aggregates;

/// <summary>
/// Profile of a user of the platform inside the domain: the role the user holds
/// and the clinic or client the user belongs to.
///
/// The credentials themselves are not kept here. Identity is delegated to
/// Supabase Auth, which custodies the e-mail and the password and issues the
/// access token; this aggregate holds the identifier of that account
/// (<see cref="Id"/>, the same value as the identifier in the identity
/// provider) and everything the domain needs to decide what the user may do.
/// </summary>
public class UserProfile
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public Role Role { get; private set; }
    public Guid? ClinicId { get; private set; }
    public Guid? ClientId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Required by Entity Framework Core.
    private UserProfile() { }

    public UserProfile(Guid authUserId, string email, string fullName, Role role, Guid? clinicId, Guid? clientId)
    {
        if (role == Role.ClinicStaff && clinicId is null)
            throw new IncompleteUserProfileException("El personal de clínica debe estar asociado a una clínica.");

        if (role == Role.PetOwner && clientId is null)
            throw new IncompleteUserProfileException("El dueño de mascota debe estar asociado a un cliente.");

        Id = authUserId;
        Email = email.Trim().ToLowerInvariant();
        FullName = fullName.Trim();
        Role = role;
        ClinicId = clinicId;
        ClientId = clientId;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Only the staff of the clinic registers clinical information. The owner of
    /// the pet consults it (US05-E1).
    /// </summary>
    public bool CanRegisterClinicalData() => Role == Role.ClinicStaff;
}

public class IncompleteUserProfileException(string message) : InvalidDomainDataException(message)
{
    public override string Code => "incomplete-user-profile";
}

/// <summary>
/// The credentials do not correspond to any user of the platform. Raised from
/// the answer of the identity provider and answered with 401 (TS01-E2).
/// </summary>
public class InvalidCredentialsException()
    : DomainException("Las credenciales enviadas no corresponden a ningún usuario de la plataforma.")
{
    public override string Code => "invalid-credentials";
}
