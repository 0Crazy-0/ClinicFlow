using ClinicFlow.Application.ClinicalFormTemplates.Commands.Shared;
using MediatR;

namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.CreateClinicalFormTemplate;

public sealed record CreateClinicalFormTemplateCommand(
    string Code,
    string Name,
    string Description,
    string JsonSchemaDefinition
) : IRequest<Guid>, IClinicalFormTemplateCommand;
