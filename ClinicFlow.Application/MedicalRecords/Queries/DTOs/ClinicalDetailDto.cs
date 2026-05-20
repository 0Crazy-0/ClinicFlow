namespace ClinicFlow.Application.MedicalRecords.Queries.DTOs;

/// <param name="TemplateCode">The unique business key code of the template used (e.g. "BLOOD_PRESS").</param>
/// <param name="JsonDataPayload">The raw JSON payload containing the form values, validated against the template's schema.</param>
public sealed record ClinicalDetailDto(string TemplateCode, string JsonDataPayload);
