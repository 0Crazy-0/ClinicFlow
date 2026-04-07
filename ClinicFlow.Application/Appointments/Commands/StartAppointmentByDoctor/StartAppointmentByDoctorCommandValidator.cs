using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.StartAppointmentByDoctor;

public class StartAppointmentByDoctorCommandValidator
    : AbstractValidator<StartAppointmentByDoctorCommand>
{
    public StartAppointmentByDoctorCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage("Appointment ID is required.");

        RuleFor(x => x.InitiatorUserId).NotEmpty().WithMessage("Initiator User ID is required.");
    }
}
