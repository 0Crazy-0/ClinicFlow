using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Scheduling;

namespace ClinicFlow.Domain.ValueObjects;

public record TimeRange
{
    public TimeOnly Start { get; }

    public TimeOnly End { get; }

    public TimeSpan Duration => End - Start;

    private TimeRange(TimeOnly start, TimeOnly end)
    {
        Start = start;
        End = end;
    }

    public static TimeRange Create(TimeOnly start, TimeOnly end)
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
