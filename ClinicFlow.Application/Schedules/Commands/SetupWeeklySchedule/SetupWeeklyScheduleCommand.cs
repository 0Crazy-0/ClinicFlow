using MediatR;

namespace ClinicFlow.Application.Schedules.Commands.SetupWeeklySchedule;

public sealed record ScheduleSlot(DayOfWeek Weekday, TimeOnly StartTime, TimeOnly EndTime);

public sealed record SetupWeeklyScheduleCommand(Guid DoctorId, IReadOnlyList<ScheduleSlot> Slots)
    : IRequest<IReadOnlyList<Guid>>;
