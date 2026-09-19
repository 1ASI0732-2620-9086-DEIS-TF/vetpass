namespace VetPass.API.Shared.Interfaces.ASP.Security;

/// <summary>
/// The two permission levels of the platform. Clinical information is
/// registered only under the first one; the second one only consults.
/// </summary>
public static class AuthorizationPolicies
{
    public const string ClinicStaff = "ClinicStaff";
    public const string PetOwner = "PetOwner";
}
