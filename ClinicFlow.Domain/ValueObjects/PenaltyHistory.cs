using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.ValueObjects;

/// <summary>
/// Encapsulates the analysis of a patient's penalty history,
/// providing semantic queries and escalation logic for the penalty workflow.
/// </summary>
public record PenaltyHistory
{
    private static readonly BlockDuration[] EscalationLadder =
    [
        BlockDuration.Minor,
        BlockDuration.Moderate,
        BlockDuration.Severe,
    ];

    private readonly IReadOnlyList<PatientPenalty> _penalties;

    public PenaltyHistory(IReadOnlyList<PatientPenalty> penalties) => _penalties = penalties;

    public bool HasPriorWarnings => _penalties.Any(p => p.Type is PenaltyType.Warning);

    /// <summary>
    /// Regardless of whether they are active, expired, or removed.
    /// </summary>
    public int TotalHistoricalBlocks => _penalties.Count(p => p.Type is PenaltyType.TemporaryBlock);

    public bool IsCurrentlyBlocked(DateTime referenceTime) =>
        _penalties.Any(p =>
            !p.IsRemoved
            && p.Type is PenaltyType.TemporaryBlock
            && p.BlockedUntil.HasValue
            && p.BlockedUntil.Value > referenceTime
        );

    /// <summary>
    /// Evaluates historical offenses to escalate the penalty, capping the maximum duration at <see cref="BlockDuration.Severe"/>.
    /// </summary>
    public BlockDuration DetermineNextBlockDuration()
    {
        var escalationLevel = Math.Min(TotalHistoricalBlocks, EscalationLadder.Length - 1);

        return EscalationLadder[escalationLevel];
    }
}
