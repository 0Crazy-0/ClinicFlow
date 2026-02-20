using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

public record MedicalLicenseNumber
{
    public string Value { get; }

    private MedicalLicenseNumber(string value)
    {
        Value = value;
    }
    
    // Factory Method
    internal static MedicalLicenseNumber Create(string licenseNumber)
    {
        if (string.IsNullOrWhiteSpace(licenseNumber)) throw new BusinessRuleValidationException("Medical license number cannot be empty.");

        var trimmed = licenseNumber.Trim();

        if (trimmed.Length < 4) throw new BusinessRuleValidationException("Medical license number is too short.");

        return new MedicalLicenseNumber(trimmed);
    }

    public override string ToString() => Value;

}
