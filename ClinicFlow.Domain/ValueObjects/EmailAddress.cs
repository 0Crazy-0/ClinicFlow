using System.Text.RegularExpressions;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

public partial record EmailAddress
{
    private const string EmailRegexPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    [GeneratedRegex(EmailRegexPattern)]
    private static partial Regex EmailRegex();

    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }
    
    // Factory Method
    internal static EmailAddress Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new BusinessRuleValidationException("Email address cannot be empty.");

        email = email.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(email)) throw new BusinessRuleValidationException("Invalid email address format.");

        return new EmailAddress(email);
    }

    public override string ToString() => Value;

}
