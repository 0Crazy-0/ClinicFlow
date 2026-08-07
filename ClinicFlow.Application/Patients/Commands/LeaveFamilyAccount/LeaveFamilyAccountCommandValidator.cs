using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Patients.Commands.LeaveFamilyAccount;

public sealed class LeaveFamilyAccountCommandValidator
    : AbstractValidator<LeaveFamilyAccountCommand>
{
    public LeaveFamilyAccountCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.InitiatorUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
