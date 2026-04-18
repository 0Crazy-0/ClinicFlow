using FluentValidation;

namespace ClinicFlow.Application.Appointments.Queries.GetAppointmentsByDateRange;

public class GetAppointmentsByDateRangeQueryValidator
    : AbstractValidator<GetAppointmentsByDateRangeQuery>
{
    public GetAppointmentsByDateRangeQueryValidator()
    {
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty();
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("EndDate must be greater than or equal to StartDate.");
    }
}
