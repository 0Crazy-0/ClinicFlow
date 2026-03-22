using FluentValidation;
using ClinicFlow.Domain.Common;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointmentByDoctor;

public class CancelAppointmentByDoctorCommandValidator : AbstractValidator<CancelAppointmentByDoctorCommand>
{
    public CancelAppointmentByDoctorCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.InitiatorUserId).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
