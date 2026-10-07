using VetPass.API.Shared.Domain.Exceptions;

namespace VetPass.API.IAM.Domain.Services;

/// <summary>
/// Minimum a password chosen by a user must meet: at least eight characters,
/// with letters and digits, and different from the one it replaces.
/// </summary>
public static class PasswordPolicy
{
    public const int MinimumLength = 8;

    public static void Ensure(string? password, string? current)
    {
        if (string.IsNullOrEmpty(password) || password.Length < MinimumLength ||
            !password.Any(char.IsLetter) || !password.Any(char.IsDigit))
            throw new WeakPasswordException();

        if (password == current)
            throw new PasswordNotChangedException();
    }
}

public class WeakPasswordException()
    : InvalidDomainDataException(
        $"La contraseña debe tener al menos {PasswordPolicy.MinimumLength} caracteres, con letras y números.")
{
    public override string Code => "weak-password";
}

public class PasswordNotChangedException()
    : InvalidDomainDataException("La contraseña nueva debe ser distinta de la actual.")
{
    public override string Code => "password-not-changed";
}

/// <summary>
/// The current password given to authorise a change does not match. It is not
/// a failed sign-in: it is answered with 400, and not with 401, so that the
/// applications do not take it as an expired session and close it.
/// </summary>
public class IncorrectCurrentPasswordException()
    : InvalidDomainDataException("La contraseña actual no es correcta.")
{
    public override string Code => "invalid-current-password";
}
