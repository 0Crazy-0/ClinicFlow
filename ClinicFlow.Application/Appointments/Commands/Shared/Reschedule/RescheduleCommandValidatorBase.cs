using ClinicFlow.Domain.Common;
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
            .WithMessage(DomainErrors.Validation.ValueMustBeInFuture);
        RuleFor(x => x.NewStartTime)
            .LessThan(x => x.NewEndTime)
            .WithMessage(DomainErrors.Validation.StartTimeMustBeBeforeEndTime);
        RuleFor(x => x.NewEndTime)
            .GreaterThan(x => x.NewStartTime)
            .WithMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }
}
