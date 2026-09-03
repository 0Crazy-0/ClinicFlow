using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using FluentValidation;

namespace ClinicFlow.Application.FamilyMemberships.Commands.ChangeAccessLevel;

public sealed class ChangeAccessLevelCommandValidator : AbstractValidator<ChangeAccessLevelCommand>
{
    public ChangeAccessLevelCommandValidator()
    {
        RuleFor(x => x.RequesterUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.TargetUserId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.PatientId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.NewAccessLevel)
            .IsInEnum()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue)
            .NotEqual(FamilyMembershipAccessLevel.Unspecified)
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }
}
