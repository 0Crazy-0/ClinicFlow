using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

/// <summary>
/// Value object representing a validated blood type (e.g., A+, O−, AB+).
/// </summary>
public record BloodType
{
    public string Value { get; }

    private static readonly HashSet<string> ValidBloodTypes = ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"];

    private BloodType(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a <see cref="BloodType"/> after validating and normalizing the input.
    /// </summary>
    /// <exception cref="BusinessRuleValidationException">Thrown when the value is empty or not a recognized blood type.</exception>
    internal static BloodType Create(string bloodType)
    {
        if (string.IsNullOrWhiteSpace(bloodType)) throw new BusinessRuleValidationException(DomainErrors.Validation.ValueRequired);

        var normalizedType = bloodType.Trim().ToUpperInvariant();

        if (!ValidBloodTypes.Contains(normalizedType))
            throw new BusinessRuleValidationException(DomainErrors.Validation.InvalidBloodType);

        return new BloodType(normalizedType);
    }

    /// <inheritdoc/>
    public override string ToString() => Value;

}
