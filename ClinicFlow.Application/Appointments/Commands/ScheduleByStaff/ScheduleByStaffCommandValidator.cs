using ClinicFlow.Application.Appointments.Commands.Shared.Schedule;
using FluentValidation;

namespace ClinicFlow.Application.Appointments.Commands.ScheduleByStaff;

public class ScheduleByStaffCommandValidator : ScheduleCommandValidatorBase<ScheduleByStaffCommand>
{
    public ScheduleByStaffCommandValidator()
    {
        RuleFor(x => x.DoctorId).NotEmpty();
    }
}
