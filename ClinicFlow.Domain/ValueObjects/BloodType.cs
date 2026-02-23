using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

/// <summary>
/// Value object representing a validated blood type (e.g., A+, O−, AB+).
/// </summary>
public record BloodType
{
    /// <summary>
    /// The normalized blood type string.
    /// </summary>
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
        if (string.IsNullOrWhiteSpace(bloodType)) throw new BusinessRuleValidationException("Blood type cannot be empty.");

        var normalizedType = bloodType.Trim().ToUpperInvariant();

        if (!ValidBloodTypes.Contains(normalizedType))
            throw new BusinessRuleValidationException($"Invalid blood type: {bloodType}. Valid types are: {string.Join(", ", ValidBloodTypes)}");

        return new BloodType(normalizedType);
    }

    /// <inheritdoc/>
    public override string ToString() => Value;

}
