using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Penalties.Queries.GetPenaltiesByPatientId;

public class GetPenaltiesByPatientIdQueryValidator : AbstractValidator<GetPenaltiesByPatientIdQuery>
{
    public GetPenaltiesByPatientIdQueryValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
