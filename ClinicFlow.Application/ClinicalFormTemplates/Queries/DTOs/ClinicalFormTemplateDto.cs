namespace ClinicFlow.Application.ClinicalFormTemplates.Queries.DTOs;

/// <param name="Code">The unique business key for the template (e.g. "BLOOD_PRESS").</param>
/// <param name="JsonSchemaDefinition">The raw JSON schema Draft-07 compliant string used to validate form submissions.</param>
/// <param name="IsDeleted">A value indicating whether the template has been soft-deleted.</param>
public sealed record ClinicalFormTemplateDto(
    Guid Id,
    string Code,
    string Name,
    string Description,
    string JsonSchemaDefinition,
    bool IsDeleted
);
