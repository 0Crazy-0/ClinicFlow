using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Patients.Commands.Shared.CreatePatient;

public abstract class CreatePatientCommandValidatorBase<TCommand> : AbstractValidator<TCommand>
    where TCommand : ICreatePatientCommand
{
    protected CreatePatientCommandValidatorBase()
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
            .LessThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage(DomainErrors.Validation.ValueCannotBeInFuture);
    }
}
