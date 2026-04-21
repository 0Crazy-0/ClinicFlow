using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Domain service that manages the patient penalty workflow, applying warnings
/// and automatically issuing progressive temporary blocks based on penalty history.
/// </summary>
public static class PatientPenaltyService
{
    /// <summary>
    /// Applies a penalty to the patient based on their penalty history,
    /// escalating from a warning to progressively longer blocks.
    /// </summary>
    /// <returns>A collection of newly generated penalties that need to be persisted.</returns>
    public static IEnumerable<PatientPenalty> ApplyPenalty(
        Guid patientId,
        IReadOnlyList<PatientPenalty> existingPenalties,
        Guid? appointmentId,
        string reason,
        DateTime referenceTime
    )
    {
        var history = new PenaltyHistory(existingPenalties);
        var penaltiesToApply = new List<PatientPenalty>();

        var warning = PatientPenalty.CreateAutomaticWarning(patientId, appointmentId, reason);
        penaltiesToApply.Add(warning);

        if (!history.HasPriorWarnings || history.IsCurrentlyBlocked(referenceTime))
            return penaltiesToApply;

        var duration = history.DetermineNextBlockDuration();

        var block = PatientPenalty.CreateAutomaticBlock(
            patientId,
            PenaltyReasons.AutomaticBlock,
            referenceTime.Date.AddDays((int)duration),
            referenceTime
        );

        penaltiesToApply.Add(block);

        return penaltiesToApply;
    }
}
