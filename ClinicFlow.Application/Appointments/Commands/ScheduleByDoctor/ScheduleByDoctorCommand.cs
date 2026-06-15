using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ScheduleByDoctor;

public sealed record ScheduleByDoctorCommand(
    Guid InitiatorUserId,
    Guid TargetPatientId,
    Guid AppointmentTypeId,
    DateTime ScheduledDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsOverbook,
    bool HasGuardianConsentVerified
) : IRequest<Guid> { }
