namespace ClinicFlow.Domain.Entities.ClinicalDetails;

/// <summary>
/// Base interface for dynamic clinical details added to a medical record.
/// </summary>
public interface IClinicalDetailRecord
{
    /// <summary>
    /// The code of the template this record is fulfilling.
    /// </summary>
    string TemplateCode { get; }

    /// <summary>
    /// The standardized JSON payload data representing the clinical record.
    /// </summary>
    string JsonDataPayload { get; }
}
