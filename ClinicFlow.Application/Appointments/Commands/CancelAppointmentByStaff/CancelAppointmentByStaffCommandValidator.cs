using FluentValidation;
using ClinicFlow.Domain.Common;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointmentByStaff;

public class CancelAppointmentByStaffCommandValidator : AbstractValidator<CancelAppointmentByStaffCommand>
{
    public CancelAppointmentByStaffCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.InitiatorUserId).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.Reason).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired).MaximumLength(500);
    }
}
