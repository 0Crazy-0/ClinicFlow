using FluentValidation;

namespace ClinicFlow.Application.Schedules.Queries.GetScheduleById;

public class GetScheduleByIdQueryValidator : AbstractValidator<GetScheduleByIdQuery>
{
    public GetScheduleByIdQueryValidator()
    {
        RuleFor(x => x.ScheduleId).NotEmpty();
    }
}
