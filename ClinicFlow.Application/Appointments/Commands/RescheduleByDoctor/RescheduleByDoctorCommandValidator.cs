using ClinicFlow.Application.Appointments.Commands.Shared.Reschedule;

namespace ClinicFlow.Application.Appointments.Commands.RescheduleByDoctor;

public class RescheduleByDoctorCommandValidator(TimeProvider timeProvider)
    : RescheduleCommandValidatorBase<RescheduleByDoctorCommand>(timeProvider) { }
