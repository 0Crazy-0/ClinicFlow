using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.DeactivateClinicalFormTemplate;

public class DeactivateClinicalFormTemplateCommandValidator
    : AbstractValidator<DeactivateClinicalFormTemplateCommand>
{
    public DeactivateClinicalFormTemplateCommandValidator()
    {
        RuleFor(x => x.TemplateId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
