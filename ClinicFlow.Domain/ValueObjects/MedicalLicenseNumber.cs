using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

public record MedicalLicenseNumber
{
    public const int MinimumLength = 4;
    public const int MaximumLength = 15;

    public string Value { get; }

    private MedicalLicenseNumber(string value)
    {
        Value = value;
    }

    internal static MedicalLicenseNumber Create(string licenseNumber)
    {
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new BusinessRuleValidationException(DomainErrors.Validation.ValueRequired);

        var trimmed = licenseNumber.Trim();

        if (trimmed.Length < MinimumLength)
            throw new BusinessRuleValidationException(DomainErrors.Validation.ValueTooShort);

        if (trimmed.Length > MaximumLength)
            throw new BusinessRuleValidationException(DomainErrors.Validation.ValueTooLong);

        return new MedicalLicenseNumber(trimmed);
    }

    /// <inheritdoc/>
    public override string ToString() => Value;
}
