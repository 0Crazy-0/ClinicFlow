using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Penalties.Commands.BlockPatient;

public class BlockPatientCommandValidator : AbstractValidator<BlockPatientCommand>
{
    public BlockPatientCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.Reason).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.Duration).IsInEnum().WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }
}
