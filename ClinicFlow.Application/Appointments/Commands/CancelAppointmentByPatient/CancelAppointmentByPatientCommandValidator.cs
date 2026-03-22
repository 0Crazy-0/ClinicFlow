using FluentValidation;
using ClinicFlow.Domain.Common;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointmentByPatient;

public class CancelAppointmentByPatientCommandValidator : AbstractValidator<CancelAppointmentByPatientCommand>
{
    public CancelAppointmentByPatientCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.InitiatorUserId).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
