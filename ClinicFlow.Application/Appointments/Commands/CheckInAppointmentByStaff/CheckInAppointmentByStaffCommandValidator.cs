using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.CheckInAppointmentByStaff;

public class CheckInAppointmentByStaffCommandValidator
    : AbstractValidator<CheckInAppointmentByStaffCommand>
{
    public CheckInAppointmentByStaffCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage("Appointment ID is required.");
    }
}
