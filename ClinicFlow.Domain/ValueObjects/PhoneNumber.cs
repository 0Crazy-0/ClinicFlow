using System.Text.RegularExpressions;
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
    
    // Factory Method
    internal static PhoneNumber Create(string number)
    {
        if (string.IsNullOrWhiteSpace(number)) throw new BusinessRuleValidationException("Phone number cannot be empty.");

        var trimmed = number.Trim();

        if (!PhoneRegex().IsMatch(trimmed) || trimmed.Length < 7) throw new BusinessRuleValidationException("Invalid phone number format.");

        return new PhoneNumber(trimmed);
    }

    public override string ToString() => Value;

}
