using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Patients.Commands.Shared.CreatePatient;

/// <summary>
/// Provides base validation rules for commands creating a patient profile.
/// </summary>
public abstract class CreatePatientCommandValidatorBase<TCommand> : AbstractValidator<TCommand>
    where TCommand : ICreatePatientCommand
{
    /// <param name="timeProvider">The time provider utilized for date validations.</param>
    protected CreatePatientCommandValidatorBase(TimeProvider timeProvider)
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
    }
}
