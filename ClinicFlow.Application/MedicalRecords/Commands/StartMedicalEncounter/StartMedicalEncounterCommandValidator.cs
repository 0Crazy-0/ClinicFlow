using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.MedicalRecords.Commands.StartMedicalEncounter;

public sealed class StartMedicalEncounterCommandValidator
    : AbstractValidator<StartMedicalEncounterCommand>
{
    public StartMedicalEncounterCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.ChiefComplaint)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }
}
