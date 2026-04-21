using System.Text.RegularExpressions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

/// <summary>
/// Value object representing a validated phone number.
/// </summary>
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

    /// <summary>
    /// Creates a <see cref="PhoneNumber"/> after validating format and length constraints.
    /// </summary>
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
