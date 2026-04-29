using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Commands.Shared;

public abstract class AppointmentTypeCommandValidatorBase<TCommand> : AbstractValidator<TCommand>
    where TCommand : IAppointmentTypeCommand
{
    protected AppointmentTypeCommandValidatorBase()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
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
