using MediatR;

namespace ClinicFlow.Application.Schedules.Commands.SetupWeeklySchedule;

public sealed record ScheduleSlot(DayOfWeek DayOfWeek, TimeSpan StartTime, TimeSpan EndTime);

public sealed record SetupWeeklyScheduleCommand(Guid DoctorId, IReadOnlyList<ScheduleSlot> Slots)
    : IRequest<IReadOnlyList<Guid>>;
