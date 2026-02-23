using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents a recurring availability slot for a doctor on a specific day of the week.
/// </summary>
public class Schedule : BaseEntity
{
    /// <summary>
    /// Identifier of the doctor who owns this schedule slot.
    /// </summary>
    public Guid DoctorId { get; init; }

    /// <summary>
    /// Day of the week for this availability slot.
    /// </summary>
    public DayOfWeek DayOfWeek { get; private set; }

    /// <summary>
    /// Start and end time of the availability window.
    /// </summary>
    public TimeRange TimeRange { get; private set; }

    /// <summary>
    /// Indicates whether this schedule slot is currently active.
    /// </summary>
    public bool IsActive { get; private set; }

    // EF Core constructor
    private Schedule() { TimeRange = null!; }

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
    internal static Schedule Create(Guid doctorId, DayOfWeek dayOfWeek, TimeRange timeRange)
    {
        if (doctorId == Guid.Empty) throw new DomainValidationException("Doctor ID cannot be empty.");
        if (!Enum.IsDefined(dayOfWeek)) throw new DomainValidationException("Invalid day of the week.");
        if (timeRange is null) throw new DomainValidationException("Time range cannot be null.");

        return new Schedule(doctorId, dayOfWeek, timeRange);
    }

    /// <summary>
    /// Checks whether this schedule slot fully covers the requested time range and is active.
    /// </summary>
    internal bool CoversTimeRange(TimeRange requestedRange) => IsActive && TimeRange.Covers(requestedRange);
}
