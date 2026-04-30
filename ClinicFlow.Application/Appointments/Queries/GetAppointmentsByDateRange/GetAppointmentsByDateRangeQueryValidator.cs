using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDateRange;

public class GetAppointmentsByDateRangeQueryValidator
    : AbstractValidator<GetAppointmentsByDateRangeQuery>
{
    public GetAppointmentsByDateRangeQueryValidator()
    {
        RuleFor(x => x.StartDate).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.EndDate)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired)
            .DependentRules(() =>
            {
                RuleFor(x => x.EndDate)
                    .GreaterThanOrEqualTo(x => x.StartDate)
                    .WithMessage(DomainErrors.Validation.InvalidDateRange);
            });
    }
}
