using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.AppointmentTypes.Commands.Shared;

/// <summary>
/// Provides base validation rules for commands managing appointment type definitions.
/// </summary>
/// <typeparam name="TCommand">The type of command being validated.</typeparam>
public abstract class AppointmentTypeCommandValidatorBase<TCommand> : AbstractValidator<TCommand>
    where TCommand : IAppointmentTypeCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppointmentTypeCommandValidatorBase{TCommand}"/> class and configures shared validation rules.
    /// </summary>
    protected AppointmentTypeCommandValidatorBase()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.DurationMinutes)
            .GreaterThan(TimeSpan.Zero)
            .WithMessage(DomainErrors.Validation.ValueMustBePositive);
    }
}
