using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ClinicFlow.Domain.Common;

public static partial class StringNormalizer
{
    [GeneratedRegex(@"\s+")]
    private static partial Regex InnerWhitespaceRegex();

    /// <summary>
    /// Normalizes free text by collapsing whitespace, lowercasing, and stripping diacritics
    /// so it can be used for search indexing or fuzzy matching.
    /// </summary>
    public static string Canonicalize(string value)
    {
        Guard.NotNullOrWhiteSpace(value);

        var collapsed = InnerWhitespaceRegex().Replace(value.Trim(), " ").ToLowerInvariant();

        var withoutMarks = collapsed
            .Normalize(NormalizationForm.FormD)
            .Where(c =>
                CharUnicodeInfo.GetUnicodeCategory(c) is not UnicodeCategory.NonSpacingMark
            );

        return string.Concat(withoutMarks).Normalize(NormalizationForm.FormC);
    }
}
