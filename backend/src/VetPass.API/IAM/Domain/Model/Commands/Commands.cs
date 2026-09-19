using VetPass.API.IAM.Domain.Model.ValueObjects;

namespace VetPass.API.IAM.Domain.Model.Commands;

public record SignInCommand(string Email, string Password);

public record RefreshSessionCommand(string RefreshToken);

/// <summary>
/// Creates an account for a user of the platform. Nobody registers by their own
/// initiative: the staff of the clinic is enrolled by the clinic, and the owner
/// of a pet receives the credentials at the reception desk (US05).
/// </summary>
public record CreateAccountCommand(string Email, string FullName, Role Role, Guid? ClinicId, Guid? ClientId);
