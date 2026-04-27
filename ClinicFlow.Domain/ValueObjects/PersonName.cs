using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

public record PersonName
{
    public string FullName { get; }

    private PersonName(string fullName)
    {
        FullName = fullName;
    }

    public static PersonName Create(string fullName)
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
