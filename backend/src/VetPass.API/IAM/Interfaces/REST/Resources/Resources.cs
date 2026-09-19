using System.ComponentModel.DataAnnotations;

namespace VetPass.API.IAM.Interfaces.REST.Resources;

public record SignInResource(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record RefreshSessionResource([Required] string RefreshToken);

public record CreateStaffAccountResource(
    [Required, EmailAddress] string Email,
    [Required] string FullName);

public record CreateOwnerAccountResource(
    [Required, EmailAddress] string Email,
    [Required] string FullName,
    [Required] Guid ClientId);

public record UserResource(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    Guid? ClinicId,
    Guid? ClientId);

public record AuthenticatedSessionResource(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType,
    UserResource User);

/// <summary>
/// Answer to the creation of an account. The temporary password travels only in
/// this answer, so that the clinic can hand it over.
/// </summary>
public record CreatedAccountResource(UserResource User, string TemporaryPassword);
