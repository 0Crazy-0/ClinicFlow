using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Entities;

public class PatientPenalty : BaseEntity
{
    public Guid PatientId { get; private set; }
    public Guid? AppointmentId { get; private set; }
    public PenaltyTypeEnum PenaltyType { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTime? BlockedUntil { get; private set; }
    // EF Core constructor
    private PatientPenalty() { }

    private PatientPenalty(Guid patientId, Guid? appointmentId, PenaltyTypeEnum penaltyType, string reason, DateTime? blockedUntil)
    {
        PatientId = patientId;
        AppointmentId = appointmentId;
        PenaltyType = penaltyType;
        Reason = reason;
        BlockedUntil = blockedUntil;
    }

    internal static PatientPenalty CreateWarning(Guid patientId, Guid? appointmentId, string reason)
    {
        return new PatientPenalty(patientId, appointmentId, PenaltyTypeEnum.Warning, reason, null);
    }

    internal static PatientPenalty CreateBlock(Guid patientId, string reason, DateTime blockedUntil)
    {
        return new PatientPenalty(patientId, null, PenaltyTypeEnum.TemporaryBlock, reason, blockedUntil);
    }
}
