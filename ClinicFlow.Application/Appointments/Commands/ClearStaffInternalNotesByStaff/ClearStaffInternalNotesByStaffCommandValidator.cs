using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.ClearStaffInternalNotesByStaff;

public sealed class ClearStaffInternalNotesByStaffCommandValidator
    : AbstractValidator<ClearStaffInternalNotesByStaffCommand>
{
    public ClearStaffInternalNotesByStaffCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}

