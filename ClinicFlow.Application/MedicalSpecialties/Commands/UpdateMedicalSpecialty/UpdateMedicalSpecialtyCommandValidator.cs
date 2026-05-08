using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.MedicalSpecialties.Commands.UpdateMedicalSpecialty;

public class UpdateMedicalSpecialtyCommandValidator
    : AbstractValidator<UpdateMedicalSpecialtyCommand>
{
    public UpdateMedicalSpecialtyCommandValidator()
    {
        RuleFor(x => x.SpecialtyId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
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
