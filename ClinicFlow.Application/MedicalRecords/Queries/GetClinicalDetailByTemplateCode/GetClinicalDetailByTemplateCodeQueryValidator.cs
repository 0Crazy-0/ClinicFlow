using FluentValidation;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetClinicalDetailByTemplateCode;

public class GetClinicalDetailByTemplateCodeQueryValidator
    : AbstractValidator<GetClinicalDetailByTemplateCodeQuery>
{
    public GetClinicalDetailByTemplateCodeQueryValidator()
    {
        RuleFor(x => x.MedicalRecordId).NotEmpty();
        RuleFor(x => x.TemplateCode).NotEmpty();
    }
}
