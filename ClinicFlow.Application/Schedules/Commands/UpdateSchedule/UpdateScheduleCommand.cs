using MediatR;

namespace ClinicFlow.Application.Schedules.Commands.UpdateSchedule;

public sealed record UpdateScheduleCommand(
    Guid DoctorId,
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime
) : IRequest<Guid>;
