using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.Entities.ClinicalDetails;

/// <summary>
/// A dynamic clinical detail record that stores data for any template type as a JSON payload.
/// </summary>
public class DynamicClinicalDetail : BaseEntity, IClinicalDetailRecord
{
    public string TemplateCode { get; private set; } = string.Empty;
    public string JsonDataPayload { get; private set; } = string.Empty;

    // EF Core constructor
    private DynamicClinicalDetail() { }

    private DynamicClinicalDetail(string templateCode, string jsonDataPayload)
    {
        TemplateCode = templateCode;
        JsonDataPayload = jsonDataPayload;
    }

    public static DynamicClinicalDetail Create(string templateCode, string jsonDataPayload) => new(templateCode, jsonDataPayload);
}
