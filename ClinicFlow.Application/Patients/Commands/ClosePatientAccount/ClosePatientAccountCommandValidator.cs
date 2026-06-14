using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Patients.Commands.ClosePatientAccount;

public sealed class ClosePatientAccountCommandValidator
    : AbstractValidator<ClosePatientAccountCommand>
{
    public ClosePatientAccountCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
