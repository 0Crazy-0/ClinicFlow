using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ScheduleByGuardian;

public sealed record ScheduleByGuardianCommand(
    Guid InitiatorUserId,
    Guid TargetPatientId,
    Guid DoctorId,
    Guid AppointmentTypeId,
    DateOnly ScheduledDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? GuardianNotes = null
) : IRequest<Guid>;
