using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.UpdateGuardianNotesByGuardian;

public sealed class UpdateGuardianNotesByGuardianCommandValidator
    : AbstractValidator<UpdateGuardianNotesByGuardianCommand>
{
    public UpdateGuardianNotesByGuardianCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.InitiatorUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.Notes).MaximumLength(500).WithMessage(DomainErrors.Validation.ValueTooLong);
    }
}
