using System.Text.RegularExpressions;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

/// <summary>
/// Value object representing a validated email address.
/// </summary>
public partial record EmailAddress
{
    private const string EmailRegexPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    [GeneratedRegex(EmailRegexPattern)]
    private static partial Regex EmailRegex();

    /// <summary>
    /// The normalized, lowercase email address string.
    /// </summary>
    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }
    
    /// <summary>
    /// Creates an <see cref="EmailAddress"/> after validating format and normalizing to lowercase.
    /// </summary>
    /// <exception cref="BusinessRuleValidationException">Thrown when the value is empty or has an invalid format.</exception>
    internal static EmailAddress Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new BusinessRuleValidationException("Email address cannot be empty.");

        email = email.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(email)) throw new BusinessRuleValidationException("Invalid email address format.");

        return new EmailAddress(email);
    }

    /// <inheritdoc/>
    public override string ToString() => Value;

}
