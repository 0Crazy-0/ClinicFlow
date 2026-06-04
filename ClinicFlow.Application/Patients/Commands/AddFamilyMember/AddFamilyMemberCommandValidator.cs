using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using FluentValidation;

namespace ClinicFlow.Application.Patients.Commands.AddFamilyMember;

public class AddFamilyMemberCommandValidator : AbstractValidator<AddFamilyMemberCommand>
{
    public AddFamilyMemberCommandValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired)
            .MinimumLength(2)
            .WithMessage(DomainErrors.Validation.ValueTooShort);
        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired)
            .MinimumLength(2)
            .WithMessage(DomainErrors.Validation.ValueTooShort);
        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(_ => timeProvider.GetUtcNow().UtcDateTime.Date)
            .WithMessage(DomainErrors.Validation.ValueCannotBeInFuture);
        RuleFor(x => x.Relationship)
            .IsInEnum()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue)
            .DependentRules(() =>
            {
                RuleFor(x => x.Relationship)
                    .NotEqual(PatientRelationship.Self)
                    .WithMessage(DomainErrors.Patient.CannotBeSelf);
            });
    }
}
