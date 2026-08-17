using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.FamilyMemberships.Commands.RevokeFamilyMember;

public sealed class RevokeFamilyMemberCommandValidator
    : AbstractValidator<RevokeFamilyMemberCommand>
{
    public RevokeFamilyMemberCommandValidator()
    {
        RuleFor(x => x.OwnerUserId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.PatientId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
