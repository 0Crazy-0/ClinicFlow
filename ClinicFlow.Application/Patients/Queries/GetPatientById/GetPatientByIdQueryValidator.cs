using FluentValidation;

namespace ClinicFlow.Application.Patients.Queries.GetPatientById;

public class GetPatientByIdQueryValidator : AbstractValidator<GetPatientByIdQuery>
{
    public GetPatientByIdQueryValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
    }
}
