using MediatR;

namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.CreateClinicalFormTemplate;

/// <summary>
/// Represents a command to create a new clinical form template.
/// </summary>
/// <param name="Code">The unique business key for the template (e.g. "BLOOD_PRESS").</param>
/// <param name="Name">The descriptive name of the template.</param>
/// <param name="Description">The description detailing the template's purpose.</param>
/// <param name="JsonSchemaDefinition">The raw JSON schema Draft-07 compliant string used to validate dynamic details.</param>
public sealed record CreateClinicalFormTemplateCommand(
    string Code,
    string Name,
    string Description,
    string JsonSchemaDefinition
) : IRequest<Guid>;
