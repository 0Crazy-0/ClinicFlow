using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.MarkAppointmentAsNoShowByStaff;

public class MarkAppointmentAsNoShowByStaffCommandValidator
    : AbstractValidator<MarkAppointmentAsNoShowByStaffCommand>
{
    public MarkAppointmentAsNoShowByStaffCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage("Appointment ID is required.");
        RuleFor(x => x.InitiatorUserId).NotEmpty().WithMessage("Initiator User ID is required.");
    }
}
