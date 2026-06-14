using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.ReassignAppointment;

public sealed class ReassignAppointmentCommandValidator
    : AbstractValidator<ReassignAppointmentCommand>
{
    public ReassignAppointmentCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.NewDoctorId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.NewDate)
            .GreaterThanOrEqualTo(_ => timeProvider.GetUtcNow().UtcDateTime.Date)
            .WithMessage(DomainErrors.Validation.ValueMustBeInFuture);
        RuleFor(x => x.NewEndTime)
            .GreaterThan(x => x.NewStartTime)
            .WithMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }
}
