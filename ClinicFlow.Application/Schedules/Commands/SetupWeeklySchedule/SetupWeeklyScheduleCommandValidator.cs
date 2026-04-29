using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Schedules.Commands.SetupWeeklySchedule;

public class SetupWeeklyScheduleCommandValidator : AbstractValidator<SetupWeeklyScheduleCommand>
{
    public SetupWeeklyScheduleCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
        RuleFor(x => x.Slots).NotEmpty().WithMessage(DomainErrors.Validation.ValueRequired);
        RuleForEach(x => x.Slots)
            .ChildRules(slot =>
            {
                slot.RuleFor(x => x.DayOfWeek)
                    .IsInEnum()
                    .WithMessage(DomainErrors.Validation.InvalidEnumValue);
                slot.RuleFor(x => x.EndTime)
                    .GreaterThan(x => x.StartTime)
                    .WithMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
            });
    }
}
