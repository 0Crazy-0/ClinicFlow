using MediatR;

namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.DeleteClinicalFormTemplate;

public sealed record DeleteClinicalFormTemplateCommand(Guid TemplateId) : IRequest;
