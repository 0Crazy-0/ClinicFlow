using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.RescheduleByGuardian;

public sealed class RescheduleByGuardianCommandValidator
    : AbstractValidator<RescheduleByGuardianCommand>
{
    public RescheduleByGuardianCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.InitiatorUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.NewDate)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime))
            .WithMessage(DomainErrors.Validation.ValueMustBeInFuture);
        RuleFor(x => x.NewEndTime)
            .GreaterThan(x => x.NewStartTime)
            .WithMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
        RuleFor(x => x.NewGuardianNotes)
            .MaximumLength(500)
            .WithMessage(DomainErrors.Validation.ValueTooLong);
    }
}
