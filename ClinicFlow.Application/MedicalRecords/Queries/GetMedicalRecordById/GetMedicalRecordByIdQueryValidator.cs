using FluentValidation;

namespace ClinicFlow.Application.MedicalRecords.Queries.GetMedicalRecordById;

public class GetMedicalRecordByIdQueryValidator : AbstractValidator<GetMedicalRecordByIdQuery>
{
    public GetMedicalRecordByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
