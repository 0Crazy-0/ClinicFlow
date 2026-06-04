using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Patients.Commands.CreateCompletePatientProfile;

public class CreateCompletePatientProfileCommandValidator
    : AbstractValidator<CreateCompletePatientProfileCommand>
{
    public CreateCompletePatientProfileCommandValidator(TimeProvider timeProvider)
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
        RuleFor(x => x.BloodType).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.EmergencyContactName)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.EmergencyContactPhone)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }
}
