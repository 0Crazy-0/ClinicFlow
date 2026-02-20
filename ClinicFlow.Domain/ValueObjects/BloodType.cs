using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

public record BloodType
{
    public string Value { get; }

    private static readonly HashSet<string> ValidBloodTypes = ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"];

    private BloodType(string value)
    {
        Value = value;
    }

    // Factory Method
    internal static BloodType Create(string bloodType)
    {
        if (string.IsNullOrWhiteSpace(bloodType)) throw new BusinessRuleValidationException("Blood type cannot be empty.");

        var normalizedType = bloodType.Trim().ToUpperInvariant();

        if (!ValidBloodTypes.Contains(normalizedType))
            throw new BusinessRuleValidationException($"Invalid blood type: {bloodType}. Valid types are: {string.Join(", ", ValidBloodTypes)}");

        return new BloodType(normalizedType);
    }

    public override string ToString() => Value;

}
