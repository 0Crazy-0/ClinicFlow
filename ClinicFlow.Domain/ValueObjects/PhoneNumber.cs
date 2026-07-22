using System.Text.RegularExpressions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

public partial record PhoneNumber
{
    public const int MinimumLength = 7;
    public const int MaximumLength = 20;

    private const string PhoneRegexPattern = @"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$";

    [GeneratedRegex(PhoneRegexPattern)]
    private static partial Regex PhoneRegex();

    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static PhoneNumber Create(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new BusinessRuleValidationException(DomainErrors.Validation.ValueRequired);

        var trimmed = number.Trim();

        if (trimmed.Length > MaximumLength)
            throw new BusinessRuleValidationException(DomainErrors.Validation.ValueTooLong);

        if (!PhoneRegex().IsMatch(trimmed) || trimmed.Length < MinimumLength)
            throw new BusinessRuleValidationException(DomainErrors.Validation.InvalidPhoneFormat);

        return new PhoneNumber(trimmed);
    }

    /// <inheritdoc/>
    public override string ToString() => Value;

    /// <remarks>
    /// Enables EF Core to translate LINQ expressions such as EF.Functions.ILike(user.PhoneNumber, pattern)
    /// into SQL. Direct nested member access (user.PhoneNumber.Value) inside a LINQ expression cannot be
    /// translated by EF Core's query compiler. Prefer accessing Value explicitly outside of query
    /// expressions for clarity.
    /// </remarks>
    public static implicit operator string(PhoneNumber phone) => phone.Value;
}
