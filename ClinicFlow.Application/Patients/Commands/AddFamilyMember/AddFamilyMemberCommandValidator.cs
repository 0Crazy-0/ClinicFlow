using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;
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
            .MinimumLength(PersonName.MinimumLength)
            .WithMessage(DomainErrors.Validation.ValueTooShort)
            .MaximumLength(PersonName.MaximumLength)
            .WithMessage(DomainErrors.Validation.ValueTooLong);
        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired)
            .MinimumLength(PersonName.MinimumLength)
            .WithMessage(DomainErrors.Validation.ValueTooShort)
            .MaximumLength(PersonName.MaximumLength)
            .WithMessage(DomainErrors.Validation.ValueTooLong);
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
