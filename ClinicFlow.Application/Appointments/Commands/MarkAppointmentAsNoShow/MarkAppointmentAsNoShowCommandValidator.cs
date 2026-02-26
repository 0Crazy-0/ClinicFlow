using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.MarkAppointmentAsNoShow;

public class MarkAppointmentAsNoShowCommandValidator : AbstractValidator<MarkAppointmentAsNoShowCommand>
{
    public MarkAppointmentAsNoShowCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage("Appointment ID is required.");
        RuleFor(x => x.InitiatorUserId).NotEmpty().WithMessage("Initiator User ID is required.");
    }
}
