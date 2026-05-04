namespace ClinicFlow.Domain.ValueObjects;

/// <summary>
/// Represents a token that proves regional scheduling regulations have been evaluated and passed.
/// </summary>
public record SchedulingClearance
{
    private SchedulingClearance() { }

    /// <summary>
    /// Grants the clearance token. This should only be called by authorized scheduling policies.
    /// </summary>
    public static SchedulingClearance Granted() => new();
}
