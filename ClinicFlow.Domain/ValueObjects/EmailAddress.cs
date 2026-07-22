using System.Text.RegularExpressions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

public partial record EmailAddress
{
    public const int MaximumLength = 254;

    private const string EmailRegexPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    [GeneratedRegex(EmailRegexPattern)]
    private static partial Regex EmailRegex();

    public string Value { get; }

    private EmailAddress(string value) => Value = value;

    internal static EmailAddress Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new BusinessRuleValidationException(DomainErrors.Validation.ValueRequired);

        email = email.Trim().ToLowerInvariant();

        if (email.Length > MaximumLength)
            throw new BusinessRuleValidationException(DomainErrors.Validation.ValueTooLong);

        if (!EmailRegex().IsMatch(email))
            throw new BusinessRuleValidationException(DomainErrors.Validation.InvalidEmailFormat);

        return new EmailAddress(email);
    }

    /// <inheritdoc/>
    public override string ToString() => Value;

    /// <remarks>
    /// Enables EF Core to translate LINQ expressions such as EF.Functions.ILike(user.Email, pattern)
    /// into SQL. Direct nested member access (user.Email.Value) inside a LINQ expression cannot be
    /// translated by EF Core's query compiler. Prefer accessing Value explicitly outside of query
    /// expressions for clarity.
    /// </remarks>
    public static implicit operator string(EmailAddress email) => email.Value;
}
