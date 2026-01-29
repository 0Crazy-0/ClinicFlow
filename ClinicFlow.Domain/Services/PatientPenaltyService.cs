using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces;

namespace ClinicFlow.Domain.Services;

public class PatientPenaltyService(IPatientPenaltyRepository penaltyRepository)
{
    private const int StrikesThreshold = 3;
    private const int BlockDurationDays = 30;

    public async Task ApplyPenaltyAsync(Guid patientId, Guid? appointmentId, string reason)
    {
        var newPenalty = PatientPenalty.CreateWarning(patientId, appointmentId, reason);
        
        await penaltyRepository.AddAsync(newPenalty);

        var existingPenalties = await penaltyRepository.GetByPatientIdAsync(patientId);
    
        var totalWarnings = existingPenalties.Count(p => p.PenaltyType is PenaltyTypeEnum.Warning);

        if (!existingPenalties.Contains(newPenalty))
            totalWarnings++;


        if (totalWarnings >= StrikesThreshold)
        {
            var isBlocked = existingPenalties.Any(p => p.PenaltyType is PenaltyTypeEnum.TemporaryBlock && p.BlockedUntil > DateTime.UtcNow);

            if (!isBlocked)
            {
                var block = PatientPenalty.CreateBlock(patientId, "Automatic block due to 3 strikes", DateTime.UtcNow.AddDays(BlockDurationDays));
                
                await penaltyRepository.AddAsync(block);
            }
        }
    }
}
