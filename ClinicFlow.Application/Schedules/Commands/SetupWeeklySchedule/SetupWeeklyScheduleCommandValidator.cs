using FluentValidation;

namespace ClinicFlow.Application.Schedules.Commands.SetupWeeklySchedule;

public class SetupWeeklyScheduleCommandValidator : AbstractValidator<SetupWeeklyScheduleCommand>
{
    public SetupWeeklyScheduleCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.Slots).NotEmpty();

        RuleForEach(x => x.Slots)
            .ChildRules(slot =>
            {
                slot.RuleFor(x => x.DayOfWeek).IsInEnum();
                slot.RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime);
            });
    }
}
