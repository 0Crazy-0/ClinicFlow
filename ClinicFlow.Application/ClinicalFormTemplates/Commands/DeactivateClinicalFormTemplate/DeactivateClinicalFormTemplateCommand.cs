using MediatR;

namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.DeactivateClinicalFormTemplate;

public sealed record DeactivateClinicalFormTemplateCommand(Guid TemplateId) : IRequest;
