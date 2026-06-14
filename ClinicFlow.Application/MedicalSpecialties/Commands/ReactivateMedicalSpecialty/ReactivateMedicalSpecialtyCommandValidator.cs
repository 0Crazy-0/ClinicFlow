using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.MedicalSpecialties.Commands.ReactivateMedicalSpecialty;

public sealed class ReactivateMedicalSpecialtyCommandValidator
    : AbstractValidator<ReactivateMedicalSpecialtyCommand>
{
    public ReactivateMedicalSpecialtyCommandValidator()
    {
        RuleFor(x => x.SpecialtyId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
