using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.Shared.Reschedule;

public abstract class RescheduleCommandValidatorBase<TCommand> : AbstractValidator<TCommand>
    where TCommand : IRescheduleCommand
{
    protected RescheduleCommandValidatorBase()
    {
        RuleFor(x => x.InitiatorUserId).NotEmpty();
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.NewDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Scheduled date cannot be in the past.");
        RuleFor(x => x.NewStartTime)
            .LessThan(x => x.NewEndTime)
            .WithMessage("Start time must be before end time.");
        RuleFor(x => x.NewEndTime)
            .GreaterThan(x => x.NewStartTime)
            .WithMessage("End time must be after start time.");
    }
}
