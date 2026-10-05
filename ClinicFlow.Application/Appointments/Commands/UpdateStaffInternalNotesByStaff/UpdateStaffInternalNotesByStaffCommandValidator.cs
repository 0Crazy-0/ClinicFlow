using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.UpdateStaffInternalNotesByStaff;

public sealed class UpdateStaffInternalNotesByStaffCommandValidator
    : AbstractValidator<UpdateStaffInternalNotesByStaffCommand>
{
    public UpdateStaffInternalNotesByStaffCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.Notes).MaximumLength(500).WithMessage(DomainErrors.Validation.ValueTooLong);
    }
}
