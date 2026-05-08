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
    }
}
