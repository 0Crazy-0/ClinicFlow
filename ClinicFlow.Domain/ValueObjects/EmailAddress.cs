using System.Text.RegularExpressions;
using ClinicFlow.Domain.Common;
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

    internal static EmailAddress Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new BusinessRuleValidationException(DomainErrors.Validation.ValueRequired);

        email = email.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(email))
            throw new BusinessRuleValidationException(DomainErrors.Validation.InvalidEmailFormat);

        return new EmailAddress(email);
    }

    /// <inheritdoc/>
    public override string ToString() => Value;
}
