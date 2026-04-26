using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Services.Args.Schedule;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Domain service that orchestrates the creation of a complete weekly schedule for a doctor,
/// enforcing duplicate-day constraints across existing and new slots.
/// </summary>
public static class WeeklyScheduleSetupService
{
    /// <returns>A list of newly created <see cref="Schedule"/> entities ready for persistence.</returns>
    public static IReadOnlyList<Schedule> SetupWeeklySchedule(
        Guid doctorId,
        IReadOnlyList<Schedule> existingSchedules,
        IReadOnlyList<WeeklyScheduleSlot> slots
    )
    {
        return
        [
            .. slots.Select(slot =>
            {
                Schedule.EnsureNoDuplicateDay(existingSchedules, doctorId, slot.DayOfWeek);
                return Schedule.Create(
                    doctorId,
                    slot.DayOfWeek,
                    TimeRange.Create(slot.StartTime, slot.EndTime)
                );
            }),
        ];
    }
}
