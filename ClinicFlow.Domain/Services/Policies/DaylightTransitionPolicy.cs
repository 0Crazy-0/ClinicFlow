using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services.Policies;

/// <summary>
/// Rejects slots that cannot be mapped to a single UTC instant in the clinic timezone.
/// Applies to the entire interval, so a slot spanning a daylight transition is rejected even when both endpoints are valid.
/// </summary>
internal static class DaylightTransitionPolicy
{
    internal static void EnsureSchedulable(
        DateOnly date,
        TimeRange slot,
        TimeZoneInfo clinicTimeZone
    )
    {
        ArgumentNullException.ThrowIfNull(slot);
        ArgumentNullException.ThrowIfNull(clinicTimeZone);

        var start = date.ToDateTime(slot.Start);
        var end = date.ToDateTime(slot.End);

        if (clinicTimeZone.IsInvalidTime(start) || clinicTimeZone.IsInvalidTime(end))
            throw new BusinessRuleValidationException(DomainErrors.Scheduling.InvalidTimeSlot);

        if (clinicTimeZone.IsAmbiguousTime(start) || clinicTimeZone.IsAmbiguousTime(end))
            throw new BusinessRuleValidationException(DomainErrors.Scheduling.AmbiguousTimeSlot);

        var startOffset = clinicTimeZone.GetUtcOffset(start);
        var endOffset = clinicTimeZone.GetUtcOffset(end);

        if (endOffset > startOffset)
            throw new BusinessRuleValidationException(DomainErrors.Scheduling.InvalidTimeSlot);

        if (endOffset < startOffset)
            throw new BusinessRuleValidationException(DomainErrors.Scheduling.AmbiguousTimeSlot);
    }
}
