using ClinicFlow.Application.ClinicalFormTemplates.Commands.Shared;
using MediatR;

namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.UpdateClinicalFormTemplate;

public sealed record UpdateClinicalFormTemplateCommand(
    Guid TemplateId,
    string Name,
    string Description,
    string JsonSchemaDefinition
) : IRequest, IClinicalFormTemplateCommand;
