using System.Text.Json;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Common;

/// <summary>
/// Ensures strings contain well-formed JSON documents.
/// </summary>
internal static class JsonSyntaxGuard
{
    /// <summary>
    /// Ensures the value parses as JSON. Accepts any well-formed document, including empty objects.
    /// </summary>
    internal static void EnsureValidJson(string json)
    {
        try
        {
            using var _ = JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            throw new DomainValidationException(DomainErrors.Validation.InvalidFormat);
        }
    }
}
