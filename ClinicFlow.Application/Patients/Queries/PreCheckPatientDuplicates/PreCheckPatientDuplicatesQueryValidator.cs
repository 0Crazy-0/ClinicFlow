using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;
using FluentValidation;

namespace ClinicFlow.Application.Patients.Queries.PreCheckPatientDuplicates;

public sealed class PreCheckPatientDuplicatesQueryValidator
    : AbstractValidator<PreCheckPatientDuplicatesQuery>
{
    public PreCheckPatientDuplicatesQueryValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.ReceptionistUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
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
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime))
            .WithMessage(DomainErrors.Validation.ValueCannotBeInFuture);
        RuleFor(x => x.SexAtBirth)
            .IsInEnum()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue)
            .NotEqual(BiologicalSex.Unspecified)
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.AddressLine)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired)
            .MaximumLength(Address.LineMaximumLength)
            .WithMessage(DomainErrors.Validation.ValueTooLong);
        RuleFor(x => x.AddressState)
            .IsInEnum()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
        RuleFor(x => x.AddressPostalCode)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired)
            .MaximumLength(Address.PostalCodeMaximumLength)
            .WithMessage(DomainErrors.Validation.ValueTooLong);
        RuleFor(x => x.AddressCountry)
            .IsInEnum()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
        RuleFor(x => x.Email).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.Email)
            .MaximumLength(EmailAddress.MaximumLength)
            .WithMessage(DomainErrors.Validation.ValueTooLong);
        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired)
            .MinimumLength(PhoneNumber.MinimumLength)
            .WithMessage(DomainErrors.Validation.ValueTooShort)
            .MaximumLength(PhoneNumber.MaximumLength)
            .WithMessage(DomainErrors.Validation.ValueTooLong);
        When(
            x => x.SsnLast4 is not null,
            () =>
                RuleFor(x => x.SsnLast4!)
                    .Matches(@"^[0-9]{4}$")
                    .WithMessage(DomainErrors.Validation.InvalidValue)
        );
    }
}
