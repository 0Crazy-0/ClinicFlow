using FluentValidation;

namespace ClinicFlow.Application.Schedules.Commands.DeactivateSchedule;

public class DeactivateScheduleCommandValidator : AbstractValidator<DeactivateScheduleCommand>
{
    public DeactivateScheduleCommandValidator()
    {
        RuleFor(x => x.ScheduleId).NotEmpty();
    }
}
