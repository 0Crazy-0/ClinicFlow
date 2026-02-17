using ClinicFlow.Domain.Exceptions.Scheduling;

namespace ClinicFlow.Domain.ValueObjects;

public record TimeRange
{
    public TimeSpan Start { get; }
    public TimeSpan End { get; }
    public TimeSpan Duration => End - Start;

    public TimeRange(TimeSpan start, TimeSpan end)
    {
        if (start >= end) throw new InvalidTimeRangeException("Start time must be before end time.");

        Start = start;
        End = end;
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
