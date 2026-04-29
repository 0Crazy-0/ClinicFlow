using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.MarkAppointmentAsNoShowByDoctor;

public class MarkAppointmentAsNoShowByDoctorCommandValidator
    : AbstractValidator<MarkAppointmentAsNoShowByDoctorCommand>
{
    public MarkAppointmentAsNoShowByDoctorCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.InitiatorUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
