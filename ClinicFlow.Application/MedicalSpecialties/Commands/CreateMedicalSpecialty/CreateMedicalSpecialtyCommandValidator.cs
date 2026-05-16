using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.ValueObjects;
using FluentValidation;

namespace ClinicFlow.Application.MedicalSpecialties.Commands.CreateMedicalSpecialty;

public class CreateMedicalSpecialtyCommandValidator
    : AbstractValidator<CreateMedicalSpecialtyCommand>
{
    public CreateMedicalSpecialtyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.Description).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.TypicalDurationMinutes)
            .Must(EncounterDuration.IsValid)
            .WithMessage(DomainErrors.MedicalSpecialty.InvalidEncounterDuration);
        RuleFor(x => x.MinCancellationHours)
            .Must(x => CancellationLimit.AllowedHours.Contains(x))
            .WithMessage(DomainErrors.MedicalSpecialty.InvalidCancellationLimit);
    }
}
