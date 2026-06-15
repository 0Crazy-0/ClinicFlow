using MediatR;

namespace ClinicFlow.Application.Schedules.Commands.UpdateSchedule;

public sealed record UpdateScheduleCommand(
    Guid DoctorId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime
) : IRequest<Guid>;
