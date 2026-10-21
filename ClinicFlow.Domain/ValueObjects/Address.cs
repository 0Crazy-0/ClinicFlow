using System.Text.RegularExpressions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

/// <summary>
/// Represents a residential address. The state, the postal code and the country
/// are validated together, so an address always describes a real jurisdiction.
/// </summary>
public partial record Address
{
    public const int LineMaximumLength = 200;
    public const int PostalCodeMaximumLength = 10;

    private const string LineRegexPattern = @"^[\p{L}\p{N}][\p{L}\p{M}\p{N}\p{P}\p{Zs}]*$";
    private const string MexicoPostalCodeRegexPattern = @"^[0-9]{5}$";
    private const string UnitedStatesPostalCodeRegexPattern = @"^[0-9]{5}(-[0-9]{4})?$";
    private const string CanadaPostalCodeRegexPattern =
        @"^[ABCEGHJ-NPRSTVXY][0-9][ABCEGHJ-NPRSTV-Z] ?[0-9][ABCEGHJ-NPRSTV-Z][0-9]$";

    [GeneratedRegex(LineRegexPattern)]
    private static partial Regex LineRegex();

    [GeneratedRegex(MexicoPostalCodeRegexPattern)]
    private static partial Regex MexicoPostalCodeRegex();

    [GeneratedRegex(UnitedStatesPostalCodeRegexPattern)]
    private static partial Regex UnitedStatesPostalCodeRegex();

    [GeneratedRegex(CanadaPostalCodeRegexPattern)]
    private static partial Regex CanadaPostalCodeRegex();

    public string Line { get; }

    public AddressState State { get; }

    public string PostalCode { get; }

    public Country Country { get; }

    private Address(string line, AddressState state, string postalCode, Country country)
    {
        Line = line;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    /// <summary>
    /// Creates an address and validates that the state belongs to the given country
    /// and that the postal code matches the format used in that country.
    /// </summary>
    public static Address Create(
        string line,
        AddressState state,
        string postalCode,
        Country country
    )
    {
        Guard.NotNullOrWhiteSpace(line);
        Guard.NotNullOrWhiteSpace(postalCode);
        Guard.IsDefined(state);
        Guard.IsDefined(country);

        var trimmedLine = line.Trim();

        if (trimmedLine.Length > LineMaximumLength)
            throw new BusinessRuleValidationException(DomainErrors.Validation.ValueTooLong);

        if (!LineRegex().IsMatch(trimmedLine))
            throw new BusinessRuleValidationException(DomainErrors.Address.InvalidLineFormat);

        var normalizedPostalCode = NormalizePostalCode(postalCode, country);

        if (AddressStateCatalog.GetCountry(state) != country)
            throw new BusinessRuleValidationException(DomainErrors.Address.StateCountryMismatch);

        if (!MatchesCountryPostalCode(normalizedPostalCode, country))
            throw new BusinessRuleValidationException(DomainErrors.Address.InvalidPostalCode);

        return new Address(trimmedLine, state, normalizedPostalCode, country);
    }

    private static string NormalizePostalCode(string postalCode, Country country)
    {
        var trimmed = postalCode.Trim();

        if (trimmed.Length > PostalCodeMaximumLength)
            throw new BusinessRuleValidationException(DomainErrors.Validation.ValueTooLong);

        if (country is not Country.Canada)
            return trimmed;

        var upperCased = trimmed.ToUpperInvariant();

        return upperCased.Length == 6 ? $"{upperCased[..3]} {upperCased[3..]}" : upperCased;
    }

    private static bool MatchesCountryPostalCode(string postalCode, Country country) =>
        country switch
        {
            Country.Mexico => MexicoPostalCodeRegex().IsMatch(postalCode),
            Country.UnitedStates => UnitedStatesPostalCodeRegex().IsMatch(postalCode),
            Country.Canada => CanadaPostalCodeRegex().IsMatch(postalCode),
            // Stryker disable once Boolean: see docs/tooling/stryker.md, section "8. Address Postal Code Switch Default"
            _ => false,
        };
}
