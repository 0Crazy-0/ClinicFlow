using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Penalties.Queries.GetPenaltiesByPatientId;

public class GetPenaltiesByPatientIdQueryValidator : AbstractValidator<GetPenaltiesByPatientIdQuery>
{
    public GetPenaltiesByPatientIdQueryValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
