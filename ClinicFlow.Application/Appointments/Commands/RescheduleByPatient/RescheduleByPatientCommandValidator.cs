using ClinicFlow.Application.Appointments.Commands.Shared.Reschedule;

namespace ClinicFlow.Application.Appointments.Commands.RescheduleByPatient;

public class RescheduleByPatientCommandValidator(TimeProvider timeProvider)
    : RescheduleCommandValidatorBase<RescheduleByPatientCommand>(timeProvider) { }
