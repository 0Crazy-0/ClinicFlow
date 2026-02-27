using ClinicFlow.Domain.Exceptions.Scheduling;

namespace ClinicFlow.Domain.ValueObjects;

/// <summary>
/// Value object representing a continuous block of time defined by a start and end <see cref="TimeSpan"/>.
/// </summary>
public record TimeRange
{
    public TimeSpan Start { get; }

    public TimeSpan End { get; }

    public TimeSpan Duration => End - Start;

    private TimeRange(TimeSpan start, TimeSpan end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Creates a <see cref="TimeRange"/> after ensuring the start time precedes the end time.
    /// </summary>
    /// <exception cref="InvalidTimeRangeException">Thrown when the start time is greater than or equal to the end time.</exception>
    internal static TimeRange Create(TimeSpan start, TimeSpan end)
    {
        if (start >= end) throw new InvalidTimeRangeException("Start time must be before end time.");

        return new TimeRange(start, end);
    }

    /// <summary>
    /// Checks whether this time range overlaps with another time range.
    /// </summary>
    /// <exception cref="InvalidTimeRangeException">Thrown when the other time range is null.</exception>
    public bool OverlapsWith(TimeRange other)
    {
        if (other is null) throw new InvalidTimeRangeException("Time range cannot be null.");

        return Start < other.End && other.Start < End;
    }

    /// <summary>
    /// Checks whether this time range completely encompasses another time range.
    /// </summary>
    /// <exception cref="InvalidTimeRangeException">Thrown when the other time range is null.</exception>
    public bool Covers(TimeRange other)
    {
        if (other is null) throw new InvalidTimeRangeException("Time range cannot be null.");

        return Start <= other.Start && End >= other.End;
    }
}
