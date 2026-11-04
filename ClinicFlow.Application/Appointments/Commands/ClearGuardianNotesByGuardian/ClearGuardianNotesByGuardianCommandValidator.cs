using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.ClearGuardianNotesByGuardian;

public sealed class ClearGuardianNotesByGuardianCommandValidator
    : AbstractValidator<ClearGuardianNotesByGuardianCommand>
{
    public ClearGuardianNotesByGuardianCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.InitiatorUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
