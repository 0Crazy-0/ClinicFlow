using ClinicFlow.Application.Appointments.Commands.Shared.Schedule;

namespace ClinicFlow.Application.Appointments.Commands.ScheduleByDoctor;

public class ScheduleByDoctorCommandValidator(TimeProvider timeProvider)
    : ScheduleCommandValidatorBase<ScheduleByDoctorCommand>(timeProvider) { }
