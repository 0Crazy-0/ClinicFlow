using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Entities;

public class Schedule : BaseEntity
{
    public Guid DoctorId { get; init; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeRange TimeRange { get; private set; }
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

    // Factory Method
    internal static Schedule Create(Guid doctorId, DayOfWeek dayOfWeek, TimeRange timeRange)
    {
        if (doctorId == Guid.Empty) throw new DomainValidationException("Doctor ID cannot be empty.");
        if (!Enum.IsDefined(dayOfWeek)) throw new DomainValidationException("Invalid day of the week.");
        if (timeRange is null) throw new DomainValidationException("Time range cannot be null.");

        return new Schedule(doctorId, dayOfWeek, timeRange);
    }

    internal bool CoversTimeRange(TimeRange requestedRange) => IsActive && TimeRange.Covers(requestedRange);
}
