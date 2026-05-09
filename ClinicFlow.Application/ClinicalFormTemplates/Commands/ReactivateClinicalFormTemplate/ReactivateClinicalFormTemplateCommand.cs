using MediatR;

namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.ReactivateClinicalFormTemplate;

public sealed record ReactivateClinicalFormTemplateCommand(Guid TemplateId) : IRequest;
