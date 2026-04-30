using ClinicFlow.Application.Appointments.Commands.Shared.Schedule;
using ClinicFlow.Domain.Common;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.ScheduleByStaff;

public class ScheduleByStaffCommandValidator : ScheduleCommandValidatorBase<ScheduleByStaffCommand>
{
    public ScheduleByStaffCommandValidator(TimeProvider timeProvider)
        : base(timeProvider)
    {
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage(DomainErrors.Validation.InvalidValue);
    }
}
