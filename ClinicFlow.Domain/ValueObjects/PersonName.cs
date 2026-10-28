using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

public partial record PersonName
{
    public const int MinimumLength = 2;
    public const int MaximumLength = 100;
    public const int SearchNameMaximumLength = MaximumLength * 2 + 1;
    private const string InnerWhitespaceRegexPattern = @"\s+";

    [GeneratedRegex(InnerWhitespaceRegexPattern)]
    private static partial Regex InnerWhitespaceRegex();

    public string FirstName { get; }

    public string? MiddleName { get; }

    public string FirstSurname { get; }

    public string SearchName { get; private set; }

    private PersonName(string firstName, string? middleName, string firstSurname)
    {
        FirstName = firstName;
        MiddleName = middleName;
        FirstSurname = firstSurname;
        SearchName = $"{NormalizeForSearch(firstName)} {NormalizeForSearch(firstSurname)}";
    }

    /// <summary>
    /// Creates a structured name from its parts. Each part keeps the spelling the person
    /// entered (only trimmed, inner whitespace collapsed, composed Unicode form) so it can
    /// be displayed as-is. <see cref="SearchName"/> holds a separate normalized value
    /// (lowercase, diacritics stripped) used for retrieval that ignores accents and spacing.
    /// </summary>
    public static PersonName Create(string firstName, string? middleName, string firstSurname)
    {
        Guard.NotNullOrWhiteSpace(firstName);
        Guard.NotNullOrWhiteSpace(firstSurname);

        if (middleName is not null)
            Guard.NotNullOrWhiteSpace(middleName);

        var cleanedFirstName = CleanForDisplay(firstName);
        var cleanedMiddleName = middleName is null ? null : CleanForDisplay(middleName);
        var cleanedFirstSurname = CleanForDisplay(firstSurname);

        if (
            cleanedFirstName.Length < MinimumLength
            || cleanedFirstSurname.Length < MinimumLength
            || (cleanedMiddleName is not null && cleanedMiddleName.Length < MinimumLength)
        )
            throw new BusinessRuleValidationException(DomainErrors.Validation.ValueTooShort);

        if (
            cleanedFirstName.Length > MaximumLength
            || cleanedFirstSurname.Length > MaximumLength
            || (cleanedMiddleName is not null && cleanedMiddleName.Length > MaximumLength)
        )
            throw new BusinessRuleValidationException(DomainErrors.Validation.ValueTooLong);

        return new PersonName(cleanedFirstName, cleanedMiddleName, cleanedFirstSurname);
    }

    /// <summary>
    /// Normalizes free text (for example a search term typed by a user) so it can be
    /// compared against <see cref="SearchName"/>.
    /// </summary>
    public static string NormalizeForSearch(string value)
    {
        var lowered = CleanForDisplay(value).ToLowerInvariant();

        var withoutMarks = lowered
            .Normalize(NormalizationForm.FormD)
            .Where(c =>
                CharUnicodeInfo.GetUnicodeCategory(c) is not UnicodeCategory.NonSpacingMark
            );

        return string.Concat(withoutMarks).Normalize(NormalizationForm.FormC);
    }

    private static string CleanForDisplay(string part) =>
        InnerWhitespaceRegex().Replace(part.Trim(), " ").Normalize(NormalizationForm.FormC);

    /// <inheritdoc/>
    public override string ToString() =>
        MiddleName is null
            ? $"{FirstName} {FirstSurname}"
            : $"{FirstName} {MiddleName} {FirstSurname}";
}
