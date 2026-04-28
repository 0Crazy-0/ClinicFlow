using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.Shared.Schedule;

public abstract class ScheduleCommandValidatorBase<TCommand> : AbstractValidator<TCommand>
    where TCommand : IScheduleCommand
{
    protected ScheduleCommandValidatorBase()
    {
        RuleFor(x => x.InitiatorUserId).NotEmpty();
        RuleFor(x => x.TargetPatientId).NotEmpty();
        RuleFor(x => x.AppointmentTypeId).NotEmpty();
        RuleFor(x => x.ScheduledDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage(DomainErrors.Validation.ValueMustBeInFuture);
        RuleFor(x => x.StartTime)
            .LessThan(x => x.EndTime)
            .WithMessage(DomainErrors.Validation.StartTimeMustBeBeforeEndTime);
        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }
}
