using ClinicFlow.Application.Appointments.Commands.Shared.Schedule;
using MediatR;

namespace ClinicFlow.Application.Appointments.Commands.ScheduleByPatient;

public record ScheduleByPatientCommand(
    Guid InitiatorUserId,
    Guid TargetPatientId,
    Guid DoctorId,
    Guid AppointmentTypeId,
    DateTime ScheduledDate,
    TimeSpan StartTime,
    TimeSpan EndTime
) : IRequest<Guid>, IScheduleCommand;
