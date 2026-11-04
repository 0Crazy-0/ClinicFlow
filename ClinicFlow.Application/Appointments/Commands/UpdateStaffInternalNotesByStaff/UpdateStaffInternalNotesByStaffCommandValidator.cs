using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.UpdateStaffInternalNotesByStaff;

public sealed class UpdateStaffInternalNotesByStaffCommandValidator
    : AbstractValidator<UpdateStaffInternalNotesByStaffCommand>
{
    public UpdateStaffInternalNotesByStaffCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.Notes).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.Notes)
            .MaximumLength(Appointment.MaxNotesLength)
            .WithMessage(DomainErrors.Validation.ValueTooLong);
    }
}
