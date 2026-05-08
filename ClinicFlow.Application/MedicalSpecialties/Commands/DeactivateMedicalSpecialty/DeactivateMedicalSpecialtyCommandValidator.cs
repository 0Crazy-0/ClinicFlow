using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.MedicalSpecialties.Commands.DeactivateMedicalSpecialty;

public class DeactivateMedicalSpecialtyCommandValidator
    : AbstractValidator<DeactivateMedicalSpecialtyCommand>
{
    public DeactivateMedicalSpecialtyCommandValidator()
    {
        RuleFor(x => x.SpecialtyId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
