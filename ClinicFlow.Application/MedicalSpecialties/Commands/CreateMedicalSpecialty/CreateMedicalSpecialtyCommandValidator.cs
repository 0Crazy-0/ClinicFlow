using ClinicFlow.Domain.Common;
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
            .GreaterThan(0)
            .WithMessage(DomainErrors.Validation.ValueMustBePositive);
        RuleFor(x => x.MinCancellationHours)
            .GreaterThanOrEqualTo(0)
            .WithMessage(DomainErrors.Validation.ValueCannotBeNegative);
    }
}
