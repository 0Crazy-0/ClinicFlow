using ClinicFlow.Domain.Common;
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

    internal static TimeRange Create(TimeSpan start, TimeSpan end)
    {
        if (start >= end)
            throw new InvalidTimeRangeException(DomainErrors.Schedule.InvalidTimeRange);

        return new TimeRange(start, end);
    }

    public bool OverlapsWith(TimeRange other)
    {
        if (other is null)
            throw new InvalidTimeRangeException(DomainErrors.General.RequiredFieldNull);

        return Start < other.End && other.Start < End;
    }

    public bool Covers(TimeRange other)
    {
        if (other is null)
            throw new InvalidTimeRangeException(DomainErrors.General.RequiredFieldNull);

        return Start <= other.Start && End >= other.End;
    }
}
