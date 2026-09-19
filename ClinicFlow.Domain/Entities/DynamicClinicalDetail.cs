using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// A dynamic clinical detail record that stores data for any template type as a JSON payload.
/// </summary>
public class DynamicClinicalDetail : BaseEntity
{
    // Stryker disable once String
    public string TemplateCode { get; private set; } = string.Empty;

    // Stryker disable once String
    public string JsonDataPayload { get; private set; } = string.Empty;

    // EF Core constructor
    private DynamicClinicalDetail() { }

    private DynamicClinicalDetail(string templateCode, string jsonDataPayload)
    {
        TemplateCode = templateCode;
        JsonDataPayload = jsonDataPayload;
    }

    /// <summary>
    /// Creates a clinical detail record after validating required fields and JSON syntax.
    /// </summary>
    public static DynamicClinicalDetail Create(string templateCode, string jsonDataPayload)
    {
        if (string.IsNullOrWhiteSpace(templateCode))
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (string.IsNullOrWhiteSpace(jsonDataPayload))
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        JsonSyntaxGuard.EnsureValidJson(jsonDataPayload);

        return new(templateCode, jsonDataPayload);
    }
}
