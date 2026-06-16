using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.ScheduleByStaff;

public sealed class ScheduleByStaffCommandValidator : AbstractValidator<ScheduleByStaffCommand>
{
    public ScheduleByStaffCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.InitiatorUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.TargetPatientId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.AppointmentTypeId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.ScheduledDate)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime))
            .WithMessage(DomainErrors.Validation.ValueMustBeInFuture);
        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime)
            .WithMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }
}
