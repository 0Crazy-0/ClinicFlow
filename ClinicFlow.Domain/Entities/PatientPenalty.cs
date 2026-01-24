using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Entities;

public class PatientPenalty : BaseEntity
{
    public Guid PatientId { get; set; }
    public Guid? AppointmentId { get; set; }

    public PenaltyType PenaltyType { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime? BlockedUntil { get; set; }
    public Patient Patient { get; set; }
    public Appointment Appointment { get; set; }
}
