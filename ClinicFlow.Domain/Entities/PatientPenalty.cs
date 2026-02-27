using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents a penalty applied to a patient, either as a warning or a temporary booking block.
/// </summary>
public class PatientPenalty : BaseEntity
{
    public Guid PatientId { get; init; }

    public Guid? AppointmentId { get; init; }

    public PenaltyType Type { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    /// <summary>
    /// UTC date and time until which the patient is blocked from booking.
    /// Only set for <see cref="PenaltyType.TemporaryBlock"/> penalties.
    /// </summary>
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

    /// <summary>
    /// Creates a warning penalty for the patient.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when the patient ID is empty or the reason is blank.</exception>
    internal static PatientPenalty CreateWarning(Guid patientId, Guid? appointmentId, string reason)
    {
        if (patientId == Guid.Empty) throw new DomainValidationException("Patient ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(reason)) throw new DomainValidationException("Penalty reason cannot be empty.");

        return new PatientPenalty(patientId, appointmentId, PenaltyType.Warning, reason, null);
    }

    /// <summary>
    /// Creates a temporary block penalty that prevents the patient from booking until the specified date.
    /// </summary>
    /// <param name="blockedUntil">UTC date and time until which the block is in effect. Must be in the future.</param>
    /// <exception cref="DomainValidationException">Thrown when the patient ID is empty, the reason is blank, or the block date is not in the future.</exception>
    internal static PatientPenalty CreateBlock(Guid patientId, string reason, DateTime blockedUntil)
    {
        if (patientId == Guid.Empty) throw new DomainValidationException("Patient ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(reason)) throw new DomainValidationException("Penalty reason cannot be empty.");
        if (blockedUntil <= DateTime.UtcNow) throw new DomainValidationException("Blocked until date must be in the future.");

        return new PatientPenalty(patientId, null, PenaltyType.TemporaryBlock, reason, blockedUntil);
    }

}