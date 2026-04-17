using FluentValidation;

namespace ClinicFlow.Application.Penalties.Queries.GetPenaltiesByPatientId;

public class GetPenaltiesByPatientIdQueryValidator : AbstractValidator<GetPenaltiesByPatientIdQuery>
{
    public GetPenaltiesByPatientIdQueryValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
    }
}
