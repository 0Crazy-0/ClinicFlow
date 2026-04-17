using FluentValidation;

namespace ClinicFlow.Application.Penalties.Commands.BlockPatient;

public class BlockPatientCommandValidator : AbstractValidator<BlockPatientCommand>
{
    public BlockPatientCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty();
        RuleFor(x => x.Duration).IsInEnum();
    }
}
