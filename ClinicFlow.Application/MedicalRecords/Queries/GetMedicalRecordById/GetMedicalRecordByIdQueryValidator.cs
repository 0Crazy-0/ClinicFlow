using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordById;

public class GetMedicalRecordByIdQueryValidator : AbstractValidator<GetMedicalRecordByIdQuery>
{
    public GetMedicalRecordByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
