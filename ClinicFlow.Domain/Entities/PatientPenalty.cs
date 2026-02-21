using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Entities;

public class PatientPenalty : BaseEntity
{
    public Guid PatientId { get; init; }
    public Guid? AppointmentId { get; init; }
    public PenaltyType Type { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTime? BlockedUntil { get; private set; }

    // EF Core constructor
    private PatientPenalty() { }

    private PatientPenalty(Guid patientId, Guid? appointmentId, PenaltyType type, string reason, DateTime? blockedUntil)
    {
        PatientId = patientId;
        AppointmentId = appointmentId;
        Type = type;
        Reason = reason;
        BlockedUntil = blockedUntil;
    }

    // Factory Methods
    internal static PatientPenalty CreateWarning(Guid patientId, Guid? appointmentId, string reason)
    {
        if (patientId == Guid.Empty) throw new DomainValidationException("Patient ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(reason)) throw new DomainValidationException("Penalty reason cannot be empty.");

        return new PatientPenalty(patientId, appointmentId, PenaltyType.Warning, reason, null);
    }

    internal static PatientPenalty CreateBlock(Guid patientId, string reason, DateTime blockedUntil)
    {
        if (patientId == Guid.Empty) throw new DomainValidationException("Patient ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(reason)) throw new DomainValidationException("Penalty reason cannot be empty.");
        if (blockedUntil <= DateTime.UtcNow) throw new DomainValidationException("Blocked until date must be in the future.");

        return new PatientPenalty(patientId, null, PenaltyType.TemporaryBlock, reason, blockedUntil);
    }

}