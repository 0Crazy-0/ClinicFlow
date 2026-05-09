using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.ReactivateClinicalFormTemplate;

public class ReactivateClinicalFormTemplateCommandValidator
    : AbstractValidator<ReactivateClinicalFormTemplateCommand>
{
    public ReactivateClinicalFormTemplateCommandValidator()
    {
        RuleFor(x => x.TemplateId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
