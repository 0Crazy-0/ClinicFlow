using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.ValueObjects;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Commands.CreateAppointmentType;

public sealed class CreateAppointmentTypeCommandValidator
    : AbstractValidator<CreateAppointmentTypeCommand>
{
    public CreateAppointmentTypeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.DurationMinutes)
            .Must(EncounterDuration.IsValid)
            .WithMessage(DomainErrors.MedicalSpecialty.InvalidEncounterDuration);
        RuleFor(x => x.MinimumAge)
            .GreaterThanOrEqualTo(AgeEligibilityPolicy.MinimumAllowedAge)
            .When(x => x.MinimumAge.HasValue)
            .WithMessage(DomainErrors.Validation.ValueCannotBeNegative)
            .LessThanOrEqualTo(AgeEligibilityPolicy.MaximumAllowedAge)
            .When(x => x.MinimumAge.HasValue)
            .WithMessage(DomainErrors.Validation.ValueExceedsMaximum);
        RuleFor(x => x.MaximumAge)
            .GreaterThanOrEqualTo(AgeEligibilityPolicy.MinimumAllowedAge)
            .When(x => x.MaximumAge.HasValue)
            .WithMessage(DomainErrors.Validation.ValueCannotBeNegative)
            .LessThanOrEqualTo(AgeEligibilityPolicy.MaximumAllowedAge)
            .When(x => x.MaximumAge.HasValue)
            .WithMessage(DomainErrors.Validation.ValueExceedsMaximum);
    }
}
