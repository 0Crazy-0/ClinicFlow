using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.FamilyMemberships.Commands.LeaveFamilyMembership;

public sealed class LeaveFamilyMembershipCommandValidator
    : AbstractValidator<LeaveFamilyMembershipCommand>
{
    public LeaveFamilyMembershipCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.PatientId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
