using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

/// <summary>
/// Value object representing a validated medical license number.
/// </summary>
public record MedicalLicenseNumber
{
    public string Value { get; }

    private MedicalLicenseNumber(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a <see cref="MedicalLicenseNumber"/> after validating length constraints.
    /// </summary>
    internal static MedicalLicenseNumber Create(string licenseNumber)
    {
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new BusinessRuleValidationException(DomainErrors.Validation.ValueRequired);

        var trimmed = licenseNumber.Trim();

        if (trimmed.Length < 4)
            throw new BusinessRuleValidationException(DomainErrors.Validation.ValueTooShort);

        return new MedicalLicenseNumber(trimmed);
    }

    /// <inheritdoc/>
    public override string ToString() => Value;
}
