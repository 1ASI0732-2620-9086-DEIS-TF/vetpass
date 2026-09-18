using VetPass.API.Shared.Domain.Exceptions;

namespace VetPass.API.IAM.Domain.Model.ValueObjects;

/// <summary>
/// Role of a user of the platform. Clinical information is registered only by
/// the staff of the clinic; the owner of the pet holds consultation permissions
/// alone, as declared in section 1.2.1 of the report.
/// </summary>
public enum Role
{
    ClinicStaff = 1,
    PetOwner = 2
}

public static class RoleExtensions
{
    public const string ClinicStaffClaim = "ClinicStaff";
    public const string PetOwnerClaim = "PetOwner";

    public static Role ToRole(this string? value)
    {
        return value?.Trim() switch
        {
            ClinicStaffClaim or "clinic_staff" => Role.ClinicStaff,
            PetOwnerClaim or "pet_owner" => Role.PetOwner,
            _ => throw new UnknownRoleException(value)
        };
    }

    public static string ToClaimValue(this Role role) =>
        role == Role.ClinicStaff ? ClinicStaffClaim : PetOwnerClaim;
}

public class UnknownRoleException(string? value)
    : InvalidDomainDataException($"El rol '{value}' no corresponde a ninguno de los roles de la plataforma.")
{
    public override string Code => "unknown-role";
}
