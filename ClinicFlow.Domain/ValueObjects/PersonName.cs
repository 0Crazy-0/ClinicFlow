using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

public record PersonName
{
    public string FullName { get; }

    private PersonName(string fullName)
    {
        FullName = fullName;
    }
    
    // Factory Method
    internal static PersonName Create(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new BusinessRuleValidationException("Name cannot be empty.");

        var trimmed = fullName.Trim();

        if (trimmed.Length < 2) throw new BusinessRuleValidationException("Name is too short.");

        return new PersonName(trimmed);
    }

    public override string ToString() => FullName;

}
