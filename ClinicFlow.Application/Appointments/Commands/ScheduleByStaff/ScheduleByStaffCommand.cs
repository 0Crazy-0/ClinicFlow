using ClinicFlow.Application.Appointments.Commands.Shared.Schedule;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ScheduleByStaff;

public sealed record ScheduleByStaffCommand(
    Guid InitiatorUserId,
    Guid TargetPatientId,
    Guid DoctorId,
    Guid AppointmentTypeId,
    DateTime ScheduledDate,
    TimeSpan StartTime,
    TimeSpan EndTime,
    bool HasGuardianConsentVerified,
    bool IsOverbook
) : IRequest<Guid>, IScheduleCommand;
