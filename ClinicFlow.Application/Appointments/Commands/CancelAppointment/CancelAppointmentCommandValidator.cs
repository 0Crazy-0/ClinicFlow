using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointment;

public class CancelAppointmentCommandValidator : AbstractValidator<CancelAppointmentCommand>
{
    public CancelAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.InitiatorUserId).NotEmpty();
    }
}
