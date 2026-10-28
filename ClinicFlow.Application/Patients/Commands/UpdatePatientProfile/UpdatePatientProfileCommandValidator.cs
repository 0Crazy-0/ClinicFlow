using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.ValueObjects;
using FluentValidation;

namespace ClinicFlow.Application.Patients.Commands.UpdatePatientProfile;

public sealed class UpdatePatientProfileCommandValidator
    : AbstractValidator<UpdatePatientProfileCommand>
{
    public UpdatePatientProfileCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.BloodType).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.EmergencyContactFirstName)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired)
            .MinimumLength(PersonName.MinimumLength)
            .WithMessage(DomainErrors.Validation.ValueTooShort)
            .MaximumLength(PersonName.MaximumLength)
            .WithMessage(DomainErrors.Validation.ValueTooLong);

        When(
            x => x.EmergencyContactMiddleName is not null,
            () =>
            {
                RuleFor(x => x.EmergencyContactMiddleName)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithMessage(DomainErrors.Validation.ValueRequired)
                    .MinimumLength(PersonName.MinimumLength)
                    .WithMessage(DomainErrors.Validation.ValueTooShort)
                    .MaximumLength(PersonName.MaximumLength)
                    .WithMessage(DomainErrors.Validation.ValueTooLong);
            }
        );

        RuleFor(x => x.EmergencyContactLastName)
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
    }
}
