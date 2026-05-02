using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Patients.Commands.RemoveFamilyMember;

public class RemoveFamilyMemberCommandValidator : AbstractValidator<RemoveFamilyMemberCommand>
{
    public RemoveFamilyMemberCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.UserId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
