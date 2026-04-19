using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Commands.CreateAppointmentType;

public class CreateAppointmentTypeCommandValidator : AbstractValidator<CreateAppointmentTypeCommand>
{
    public CreateAppointmentTypeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.DurationMinutes)
            .GreaterThan(TimeSpan.Zero)
            .WithMessage(DomainErrors.Validation.ValueMustBePositive);
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
