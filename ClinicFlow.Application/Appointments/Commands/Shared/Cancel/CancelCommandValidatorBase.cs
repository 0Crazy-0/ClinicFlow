using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.Shared.Cancel;

public abstract class CancelCommandValidatorBase<TCommand> : AbstractValidator<TCommand>
    where TCommand : ICancelCommand
{
    protected CancelCommandValidatorBase()
    {
        RuleFor(x => x.AppointmentId).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.InitiatorUserId)
            .NotEmpty()
            .WithMessage(DomainErrors.Validation.ValueRequired);
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
