using MediatR;

namespace ClinicFlow.Application.Schedules.Commands.CreateSchedule;

public sealed record CreateScheduleCommand(
    Guid DoctorId,
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime
) : IRequest<Guid>;
