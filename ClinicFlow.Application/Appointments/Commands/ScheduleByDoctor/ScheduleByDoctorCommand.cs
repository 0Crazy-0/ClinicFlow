using ClinicFlow.Application.Appointments.Commands.Shared.Schedule;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ScheduleByDoctor;

public sealed record ScheduleByDoctorCommand(
    Guid InitiatorUserId,
    Guid TargetPatientId,
    Guid AppointmentTypeId,
    DateTime ScheduledDate,
    TimeSpan StartTime,
    TimeSpan EndTime,
    bool IsOverbook,
    bool HasGuardianConsentVerified
) : IRequest<Guid>, IScheduleCommand;
