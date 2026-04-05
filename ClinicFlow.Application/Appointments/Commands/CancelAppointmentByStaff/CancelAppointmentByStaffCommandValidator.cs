using ClinicFlow.Application.Appointments.Commands.Shared.Cancel;
using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.CancelAppointmentByStaff;

public class CancelAppointmentByStaffCommandValidator
    : CancelCommandValidatorBase<CancelAppointmentByStaffCommand>
{
    public CancelAppointmentByStaffCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
    }
}
