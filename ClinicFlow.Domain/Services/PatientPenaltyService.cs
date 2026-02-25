using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Domain service that manages the patient penalty workflow, applying warnings
/// and automatically issuing temporary blocks after a configurable number of strikes.
/// </summary>
/// <remarks>
/// A patient is automatically blocked for <c>30</c> days after accumulating
/// <c>3</c> warnings, unless they already have an active block.
/// </remarks>
public static class PatientPenaltyService
{
    private const int StrikesThreshold = 3;
    private const int BlockDurationDays = 30;

    /// <summary>
    /// Applies a warning penalty to the patient and, if the warning threshold is reached,
    /// creates a temporary booking block.
    /// </summary>
    public static IEnumerable<PatientPenalty> ApplyPenalty(Guid patientId, IEnumerable<PatientPenalty> existingPenalties, Guid? appointmentId, string reason)
    {
        var penaltiesToApply = new List<PatientPenalty>();
        
        var newWarning = PatientPenalty.CreateWarning(patientId, appointmentId, reason);

        penaltiesToApply.Add(newWarning);

        var totalWarnings = existingPenalties.Count(p => p.Type is PenaltyType.Warning) + 1;

        if (totalWarnings >= StrikesThreshold)
        {
            var isBlocked = existingPenalties.Any(p => p.Type is PenaltyType.TemporaryBlock && p.BlockedUntil.HasValue && p.BlockedUntil.Value > DateTime.UtcNow);

            if (!isBlocked)
            {
                var block = PatientPenalty.CreateBlock(patientId, "Automatic block due to 3 strikes", DateTime.UtcNow.AddDays(BlockDurationDays));
                penaltiesToApply.Add(block);
            }
        }

        return penaltiesToApply;
    }
}
