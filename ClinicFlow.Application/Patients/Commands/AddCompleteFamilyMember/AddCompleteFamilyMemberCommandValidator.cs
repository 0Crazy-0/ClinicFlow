using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.ValueObjects;
using FluentValidation;

namespace ClinicFlow.Application.Patients.Commands.AddCompleteFamilyMember;

public sealed class AddCompleteFamilyMemberCommandValidator
    : AbstractValidator<AddCompleteFamilyMemberCommand>
{
    public AddCompleteFamilyMemberCommandValidator(TimeProvider timeProvider)
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
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime))
            .WithMessage(DomainErrors.Validation.ValueCannotBeInFuture);
        RuleFor(x => x.BloodType).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.EmergencyContactName)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired)
            .MinimumLength(PersonName.MinimumLength)
            .WithMessage(DomainErrors.Validation.ValueTooShort)
            .MaximumLength(PersonName.MaximumLength)
            .WithMessage(DomainErrors.Validation.ValueTooLong);
        RuleFor(x => x.EmergencyContactPhone)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired)
            .MinimumLength(PhoneNumber.MinimumLength)
            .WithMessage(DomainErrors.Validation.ValueTooShort)
            .MaximumLength(PhoneNumber.MaximumLength)
            .WithMessage(DomainErrors.Validation.ValueTooLong);
        RuleFor(x => x.Relationship)
            .IsInEnum()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }
}
