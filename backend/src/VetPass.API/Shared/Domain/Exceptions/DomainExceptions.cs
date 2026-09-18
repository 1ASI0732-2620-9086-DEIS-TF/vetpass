namespace VetPass.API.Shared.Domain.Exceptions;

/// <summary>
/// Base class of every rule the domain defends. Each subclass carries a code
/// that the REST layer turns into a ProblemDetails response.
/// </summary>
public abstract class DomainException(string message) : Exception(message)
{
    public abstract string Code { get; }
}

/// <summary>
/// The data submitted cannot form a valid domain object. Answered with 400.
/// </summary>
public abstract class InvalidDomainDataException(string message) : DomainException(message);

/// <summary>
/// The data is well formed, but applying it would break a rule of the
/// vaccination schedule or of the medical record. Answered with 422, as
/// specified in TS03-E3.
/// </summary>
public abstract class DomainRuleViolationException(string message) : DomainException(message);

/// <summary>The requested resource does not exist. Answered with 404.</summary>
public class ResourceNotFoundException(string resource, Guid id)
    : DomainException($"No existe {resource} con identificador {id}.")
{
    public override string Code => "resource-not-found";
}

/// <summary>
/// The role of the authenticated user does not allow the operation, or the
/// resource belongs to someone else. Answered with 403 (US05-E2).
/// </summary>
public class ForbiddenOperationException(string message) : DomainException(message)
{
    public override string Code => "forbidden-operation";
}
