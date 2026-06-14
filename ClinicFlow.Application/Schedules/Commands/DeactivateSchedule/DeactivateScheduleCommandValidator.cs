using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Schedules.Commands.DeactivateSchedule;

public sealed class DeactivateScheduleCommandValidator
    : AbstractValidator<DeactivateScheduleCommand>
{
    public DeactivateScheduleCommandValidator()
    {
        RuleFor(x => x.ScheduleId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
