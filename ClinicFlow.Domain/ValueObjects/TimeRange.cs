using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.ValueObjects;

public class TimeRange : ValueObject
{
    public TimeSpan Start { get; }
    public TimeSpan End { get; }

    public TimeRange(TimeSpan start, TimeSpan end)
    {
        if (start >= end)
            throw new ArgumentException("Start time must be before end time.");

        Start = start;
        End = end;
    }

    public TimeSpan Duration => End - Start;

    // Check if two time ranges overlap
    public bool OverlapsWith(TimeRange other) => Start < other.End && other.Start < End;


    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Start;
        yield return End;
    }
}
