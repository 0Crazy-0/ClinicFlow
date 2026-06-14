using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.ClinicalFormTemplates.Queries.GetClinicalFormTemplateByCode;

public sealed class GetClinicalFormTemplateByCodeQueryValidator
    : AbstractValidator<GetClinicalFormTemplateByCodeQuery>
{
    public GetClinicalFormTemplateByCodeQueryValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
    }
}
