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

    /// <summary>
    /// Creates a new schedule slot for a doctor.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when the doctor ID is empty, the day of week is invalid, or the time range is null.</exception>
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
    /// Deactivates this schedule slot, preventing new appointments from being booked on this time range.
    /// Existing appointments are not affected.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when the schedule is already inactive.</exception>
    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainValidationException(DomainErrors.Schedule.AlreadyInactive);

        IsActive = false;
    }

    /// <summary>
    /// Checks whether this schedule slot fully covers the requested time range and is active.
    /// </summary>
    internal bool CoversTimeRange(TimeRange requestedRange) =>
        IsActive && TimeRange.Covers(requestedRange);

    /// <summary>
    /// Verifies that no active schedule already exists for the given doctor on the specified day.
    /// </summary>
    /// <param name="existingSchedules">The doctor's existing schedule records.</param>
    /// <param name="doctorId">The doctor's unique identifier.</param>
    /// <param name="dayOfWeek">The day of week to check for duplicates.</param>
    /// <exception cref="ScheduleAlreadyExistsException">Thrown when an active schedule already exists for the given day.</exception>
    public static void EnsureNoDuplicateDay(
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
