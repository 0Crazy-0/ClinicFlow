using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.ValueObjects;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Commands.ChangeAppointmentTypeAgePolicy;

public sealed class ChangeAppointmentTypeAgePolicyCommandValidator
    : AbstractValidator<ChangeAppointmentTypeAgePolicyCommand>
{
    public ChangeAppointmentTypeAgePolicyCommandValidator()
    {
        RuleFor(x => x.AppointmentTypeId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
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
