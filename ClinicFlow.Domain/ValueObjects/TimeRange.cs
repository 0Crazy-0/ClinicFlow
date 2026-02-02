using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions;

namespace ClinicFlow.Domain.ValueObjects;

public class TimeRange : ValueObject
{
    public TimeSpan Start { get; }
    public TimeSpan End { get; }
    public TimeSpan Duration => End - Start;

    public TimeRange(TimeSpan start, TimeSpan end)
    {
        if (start >= end)
            throw new InvalidTimeRangeException("Start time must be before end time.");

        Start = start;
        End = end;
    }

    // Check if two time ranges overlap
    public bool OverlapsWith(TimeRange other) => Start < other.End && other.Start < End;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }
}
