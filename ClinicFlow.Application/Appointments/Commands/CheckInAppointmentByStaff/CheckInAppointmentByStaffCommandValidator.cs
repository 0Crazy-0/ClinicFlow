using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.CheckInAppointmentByStaff;

public class CheckInAppointmentByStaffCommandValidator
    : AbstractValidator<CheckInAppointmentByStaffCommand>
{
    public CheckInAppointmentByStaffCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.ReceptionistNotes)
            .MaximumLength(500)
            .WithMessage(DomainErrors.Validation.ValueTooLong);
    }
}
