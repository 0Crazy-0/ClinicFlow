using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.ClinicalFormTemplates.Queries.GetClinicalFormTemplateById;

public class GetClinicalFormTemplateByIdQueryValidator
    : AbstractValidator<GetClinicalFormTemplateByIdQuery>
{
    public GetClinicalFormTemplateByIdQueryValidator()
    {
        RuleFor(x => x.ClinicalFormTemplateId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
