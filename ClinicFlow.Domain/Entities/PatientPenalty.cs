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
}
