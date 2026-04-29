using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.MarkAppointmentAsNoShowByStaff;

public class MarkAppointmentAsNoShowByStaffCommandValidator
    : AbstractValidator<MarkAppointmentAsNoShowByStaffCommand>
{
    public MarkAppointmentAsNoShowByStaffCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
