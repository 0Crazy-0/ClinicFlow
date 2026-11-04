using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.ClearPatientNotesByPatient;

public sealed class ClearPatientNotesByPatientCommandValidator
    : AbstractValidator<ClearPatientNotesByPatientCommand>
{
    public ClearPatientNotesByPatientCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.InitiatorUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
