using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointmentByDoctor;

public sealed class CancelAppointmentByDoctorCommandValidator
    : AbstractValidator<CancelAppointmentByDoctorCommand>
{
    public CancelAppointmentByDoctorCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.InitiatorUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.Reason).MaximumLength(500).WithMessage(DomainErrors.Validation.ValueTooLong);
    }
}
