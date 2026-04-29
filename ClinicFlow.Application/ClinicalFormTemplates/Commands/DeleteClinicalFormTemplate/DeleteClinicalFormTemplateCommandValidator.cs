using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.ClinicalFormTemplates.Commands.DeleteClinicalFormTemplate;

public class DeleteClinicalFormTemplateCommandValidator
    : AbstractValidator<DeleteClinicalFormTemplateCommand>
{
    public DeleteClinicalFormTemplateCommandValidator()
    {
        RuleFor(x => x.TemplateId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
