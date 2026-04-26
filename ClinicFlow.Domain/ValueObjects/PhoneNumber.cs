using System.Text.RegularExpressions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

public partial record PhoneNumber
{
    private const string PhoneRegexPattern = @"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$";

    [GeneratedRegex(PhoneRegexPattern)]
    private static partial Regex PhoneRegex();

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    internal static PhoneNumber Create(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new BusinessRuleValidationException(DomainErrors.Validation.ValueRequired);

        var trimmed = number.Trim();

        if (!PhoneRegex().IsMatch(trimmed) || trimmed.Length < 7)
            throw new BusinessRuleValidationException(DomainErrors.Validation.InvalidPhoneFormat);

        return new PhoneNumber(trimmed);
    }

    /// <inheritdoc/>
    public override string ToString() => Value;
}
