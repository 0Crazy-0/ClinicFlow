using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Penalties.Queries.GetActiveWarnings;

public sealed class GetActiveWarningsQueryValidator : AbstractValidator<GetActiveWarningsQuery>
{
    public GetActiveWarningsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
