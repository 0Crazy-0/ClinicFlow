using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Penalties.Commands.RemovePenalty;

public class RemovePenaltyCommandValidator : AbstractValidator<RemovePenaltyCommand>
{
    public RemovePenaltyCommandValidator()
    {
        RuleFor(x => x.PenaltyId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
