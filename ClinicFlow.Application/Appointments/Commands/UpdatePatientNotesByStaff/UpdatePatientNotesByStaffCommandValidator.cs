using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.UpdatePatientNotesByStaff;

public class UpdatePatientNotesByStaffCommandValidator
    : AbstractValidator<UpdatePatientNotesByStaffCommand>
{
    public UpdatePatientNotesByStaffCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.Notes).MaximumLength(500).WithMessage(DomainErrors.Validation.ValueTooLong);
    }
}
