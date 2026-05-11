namespace ClinicFlow.Application.ClinicalFormTemplates.Queries.DTOs;

public sealed record ClinicalFormTemplateDto(
    Guid Id,
    string Code,
    string Name,
    string Description,
    string JsonSchemaDefinition,
    bool IsDeleted
);
