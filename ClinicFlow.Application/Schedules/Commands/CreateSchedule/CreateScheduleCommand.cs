using MediatR;

namespace ClinicFlow.Application.Schedules.Commands.CreateSchedule;

public sealed record CreateScheduleCommand(
    Guid DoctorId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime
) : IRequest<Guid>;
