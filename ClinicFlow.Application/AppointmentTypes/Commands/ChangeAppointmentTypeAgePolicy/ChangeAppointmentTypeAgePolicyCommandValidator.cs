using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Commands.ChangeAppointmentTypeAgePolicy;

public class ChangeAppointmentTypeAgePolicyCommandValidator
    : AbstractValidator<ChangeAppointmentTypeAgePolicyCommand>
{
    public ChangeAppointmentTypeAgePolicyCommandValidator()
    {
        RuleFor(x => x.AppointmentTypeId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.MinimumAge)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinimumAge.HasValue)
            .WithMessage(DomainErrors.Validation.ValueCannotBeNegative);
        RuleFor(x => x.MaximumAge)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaximumAge.HasValue)
            .WithMessage(DomainErrors.Validation.ValueCannotBeNegative);
    }
}
