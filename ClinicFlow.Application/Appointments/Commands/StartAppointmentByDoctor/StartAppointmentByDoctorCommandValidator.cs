using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.StartAppointmentByDoctor;

public class StartAppointmentByDoctorCommandValidator
    : AbstractValidator<StartAppointmentByDoctorCommand>
{
    public StartAppointmentByDoctorCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.InitiatorUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
