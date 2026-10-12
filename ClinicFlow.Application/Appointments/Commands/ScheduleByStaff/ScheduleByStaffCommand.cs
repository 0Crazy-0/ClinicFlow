using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ScheduleByStaff;

public sealed record ScheduleByStaffCommand(
    Guid InitiatorUserId,
    Guid TargetPatientId,
    Guid DoctorId,
    Guid AppointmentTypeId,
    DateOnly ScheduledDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool HasGuardianConsentVerified
) : IRequest<Guid> { }
