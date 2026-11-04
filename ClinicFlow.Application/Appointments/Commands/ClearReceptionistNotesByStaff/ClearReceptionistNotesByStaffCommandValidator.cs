using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.ClearReceptionistNotesByStaff;

public sealed class ClearReceptionistNotesByStaffCommandValidator
    : AbstractValidator<ClearReceptionistNotesByStaffCommand>
{
    public ClearReceptionistNotesByStaffCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
