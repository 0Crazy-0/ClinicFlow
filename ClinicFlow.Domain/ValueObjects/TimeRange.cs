using ClinicFlow.Domain.Exceptions.Scheduling;

namespace ClinicFlow.Domain.ValueObjects;

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
    // Factory Method
    internal static TimeRange Create(TimeSpan start, TimeSpan end)
    {
        if (start >= end) throw new InvalidTimeRangeException("Start time must be before end time.");

        return new TimeRange(start, end);
    }

    // Check if two time ranges overlap
    public bool OverlapsWith(TimeRange other)
    {
        if (other is null) throw new InvalidTimeRangeException("Time range cannot be null.");

        return Start < other.End && other.Start < End;
    }

    public bool Covers(TimeRange other)
    {
        if (other is null) throw new InvalidTimeRangeException("Time range cannot be null.");

        return Start <= other.Start && End >= other.End;
    }
}
