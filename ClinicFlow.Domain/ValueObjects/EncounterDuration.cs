using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.ValueObjects;

/// <summary>
/// Represents the duration of a medical encounter, expressed in standardized scheduling blocks
/// </summary>
/// <remarks>
/// Only increments of 5 minutes are allowed to ensure standardized scheduling blocks.
/// </remarks>
public sealed record EncounterDuration
{
    private const int MinMinutes = 10;
    private const int MaxMinutes = 90;
    private const int IncrementMinutes = 5;

    public int Minutes { get; }

    private EncounterDuration(int minutes)
    {
        Minutes = minutes;
    }

    public static bool IsValid(int minutes) =>
        minutes >= MinMinutes && minutes <= MaxMinutes && minutes % IncrementMinutes is 0;

    public static EncounterDuration FromMinutes(int minutes)
    {
        if (!IsValid(minutes))
            throw new DomainValidationException(
                DomainErrors.MedicalSpecialty.InvalidEncounterDuration
            );

        return new EncounterDuration(minutes);
    }
}
