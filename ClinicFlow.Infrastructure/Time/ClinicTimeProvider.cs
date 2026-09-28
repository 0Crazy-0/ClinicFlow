namespace ClinicFlow.Infrastructure.Time;

/// <summary>
/// Provides time in the clinic operating timezone. Delegates UTC requests to the inner provider and
/// exposes the clinic timezone through <see cref="LocalTimeZone"/>, so <see cref="GetLocalNow"/>
/// returns clinic wall clock time. UTC values are never shifted.
/// </summary>
public sealed class ClinicTimeProvider(TimeProvider innerProvider, string timeZoneId) : TimeProvider
{
    private readonly TimeZoneInfo _clinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

    public override DateTimeOffset GetUtcNow() => innerProvider.GetUtcNow();

    public override TimeZoneInfo LocalTimeZone => _clinicTimeZone;
}
