using MediatR;
using ScheduleSlot = (
    System.DayOfWeek DayOfWeek,
    System.TimeSpan StartTime,
    System.TimeSpan EndTime
);

namespace ClinicFlow.Application.Schedules.Commands.SetupWeeklySchedule;

public sealed record SetupWeeklyScheduleCommand(Guid DoctorId, IReadOnlyList<ScheduleSlot> Slots)
    : IRequest<IReadOnlyList<Guid>>;
