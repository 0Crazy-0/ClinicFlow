using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Penalties.Queries.GetActiveBlockedPatients;

public sealed class GetActiveBlockedPatientsQueryValidator
    : AbstractValidator<GetActiveBlockedPatientsQuery>
{
    public GetActiveBlockedPatientsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
