using VetPass.API.Shared.Domain.Exceptions;

namespace VetPass.API.Vaccination.Domain.Model.ValueObjects;

/// <summary>
/// Code identifying the production batch of the vial applied. It is recorded
/// for sanitary traceability, so an empty value is not admissible.
/// </summary>
public record BatchCode
{
    public string Value { get; }

    public BatchCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidBatchCodeException();

        Value = value.Trim().ToUpperInvariant();
    }

    public override string ToString() => Value;
}

public class InvalidBatchCodeException()
    : InvalidDomainDataException("El lote de la vacuna es obligatorio y no puede estar vacío.")
{
    public override string Code => "invalid-batch-code";
}
