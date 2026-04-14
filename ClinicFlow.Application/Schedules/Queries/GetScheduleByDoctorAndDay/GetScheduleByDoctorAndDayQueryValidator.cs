using FluentValidation;

namespace ClinicFlow.Application.Schedules.Queries.GetScheduleByDoctorAndDay;

public class GetScheduleByDoctorAndDayQueryValidator
    : AbstractValidator<GetScheduleByDoctorAndDayQuery>
{
    public GetScheduleByDoctorAndDayQueryValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.DayOfWeek).IsInEnum();
    }
}
