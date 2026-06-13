using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

public record PersonName
{
    public const int MinimumLength = 2;
    public const int MaximumLength = 100;

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

        if (trimmed.Length < MinimumLength)
            throw new BusinessRuleValidationException(DomainErrors.Validation.ValueTooShort);

        if (trimmed.Length > MaximumLength)
            throw new BusinessRuleValidationException(DomainErrors.Validation.ValueTooLong);

        return new PersonName(trimmed);
    }

    /// <inheritdoc/>
    public override string ToString() => FullName;
}
