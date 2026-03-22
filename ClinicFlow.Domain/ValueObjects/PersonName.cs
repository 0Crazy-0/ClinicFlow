using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

/// <summary>
/// Value object representing a validated person's full name.
/// </summary>
public record PersonName
{
    public string FullName { get; }

    private PersonName(string fullName)
    {
        FullName = fullName;
    }

    /// <summary>
    /// Creates a <see cref="PersonName"/> after validating length constraints.
    /// </summary>
    /// <exception cref="BusinessRuleValidationException">Thrown when the value is empty or shorter than 2 characters.</exception>
    internal static PersonName Create(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new BusinessRuleValidationException(DomainErrors.Validation.ValueRequired);

        var trimmed = fullName.Trim();

        if (trimmed.Length < 2)
            throw new BusinessRuleValidationException(DomainErrors.Validation.ValueTooShort);

        return new PersonName(trimmed);
    }

    /// <inheritdoc/>
    public override string ToString() => FullName;
}
