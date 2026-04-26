using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Scheduling;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents a recurring availability slot for a doctor on a specific day of the week.
/// </summary>
public class Schedule : BaseEntity
{
    public Guid DoctorId { get; init; }

    public DayOfWeek DayOfWeek { get; private set; }

    public TimeRange TimeRange { get; private set; }

    public bool IsActive { get; private set; }

    // EF Core constructor
    private Schedule()
    {
        TimeRange = null!;
    }

    private Schedule(Guid doctorId, DayOfWeek dayOfWeek, TimeRange timeRange)
    {
        DoctorId = doctorId;
        DayOfWeek = dayOfWeek;
        TimeRange = timeRange;
        IsActive = true;
    }

    public static Schedule Create(Guid doctorId, DayOfWeek dayOfWeek, TimeRange timeRange)
    {
        if (doctorId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (!Enum.IsDefined(dayOfWeek))
            throw new DomainValidationException(DomainErrors.Schedule.InvalidDayOfWeek);
        if (timeRange is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        return new Schedule(doctorId, dayOfWeek, timeRange);
    }

    /// <summary>
    /// Marks the schedule slot as unavailable for new bookings.
    /// </summary>
    /// <remarks>
    /// Existing appointments within this slot are not affected by this status change.
    /// </remarks>
    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainValidationException(DomainErrors.Schedule.AlreadyInactive);

        IsActive = false;
    }

    internal bool CoversTimeRange(TimeRange requestedRange) =>
        IsActive && TimeRange.Covers(requestedRange);

    internal static void EnsureNoDuplicateDay(
        IReadOnlyList<Schedule> existingSchedules,
        Guid doctorId,
        DayOfWeek dayOfWeek
    )
    {
        if (existingSchedules.Any(s => s.DayOfWeek == dayOfWeek && s.IsActive))
            throw new ScheduleAlreadyExistsException(
                DomainErrors.Schedule.ScheduleAlreadyExists,
                doctorId,
                dayOfWeek
            );
    }
}
