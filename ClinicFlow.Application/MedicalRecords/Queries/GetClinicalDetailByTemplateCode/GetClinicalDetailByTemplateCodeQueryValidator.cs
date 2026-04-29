using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetClinicalDetailByTemplateCode;

public class GetClinicalDetailByTemplateCodeQueryValidator
    : AbstractValidator<GetClinicalDetailByTemplateCodeQuery>
{
    public GetClinicalDetailByTemplateCodeQueryValidator()
    {
        RuleFor(x => x.MedicalRecordId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.TemplateCode).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
    }
}
