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
    /// <summary>
    /// Creates a set of schedule slots for a doctor's weekly availability,
    /// validating no duplicate active days exist.
    /// </summary>
    /// <param name="doctorId">The doctor's unique identifier.</param>
    /// <param name="existingSchedules">The doctor's current schedule records from persistence.</param>
    /// <param name="slots">The desired weekly availability slots to create.</param>
    /// <returns>A list of newly created <see cref="Schedule"/> entities ready for persistence.</returns>
    /// <exception cref="Exceptions.Scheduling.ScheduleAlreadyExistsException">
    /// Thrown when an active schedule already exists for a requested day.
    /// </exception>
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
