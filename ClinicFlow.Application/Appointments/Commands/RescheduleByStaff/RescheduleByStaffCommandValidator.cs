using ClinicFlow.Application.Appointments.Commands.Shared.Reschedule;

namespace ClinicFlow.Application.Appointments.Commands.RescheduleByStaff;

public class RescheduleByStaffCommandValidator(TimeProvider timeProvider)
    : RescheduleCommandValidatorBase<RescheduleByStaffCommand>(timeProvider) { }
