namespace ClinicFlow.Domain.Enums;

/// <summary>
/// Defines the predefined durations (in days) for manual patient blocks issued by staff.
/// </summary>
public enum BlockDuration
{
    /// <summary>
    /// A minor block lasting 5 days.
    /// </summary>
    Minor = 5,

    /// <summary>
    /// A moderate block lasting 15 days.
    /// </summary>
    Moderate = 15,

    /// <summary>
    /// A severe block lasting 30 days.
    /// </summary>
    Severe = 30,
}
