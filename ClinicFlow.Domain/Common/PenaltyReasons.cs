namespace ClinicFlow.Domain.Common;

/// <summary>
/// Contains the standardized descriptive reasons for applying patient penalties.
/// </summary>
public static class PenaltyReasons
{
    public const string NoShow = "No show";
    public const string LateCancellation = "Late cancellation";
    public const string AutomaticBlock = "Automatic block due to 3 strikes";
}
