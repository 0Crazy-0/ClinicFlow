using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.RescheduleByStaff;

public sealed class RescheduleByStaffCommandValidator : AbstractValidator<RescheduleByStaffCommand>
{
    public RescheduleByStaffCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.InitiatorUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.NewDate)
            .GreaterThanOrEqualTo(_ => timeProvider.GetUtcNow().UtcDateTime.Date)
            .WithMessage(DomainErrors.Validation.ValueMustBeInFuture);
        RuleFor(x => x.NewEndTime)
            .GreaterThan(x => x.NewStartTime)
            .WithMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }
}
