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

    /// <summary>
    /// Indicates whether this penalty has been manually removed by staff.
    /// </summary>
    /// <remarks>
    /// A removed penalty is excluded from all active-block checks.
    /// </remarks>
    public bool IsRemoved { get; private set; }

    // EF Core constructor
    private PatientPenalty() { }

    private PatientPenalty(
        Guid patientId,
        Guid? appointmentId,
        PenaltyType type,
        string reason,
        DateTime? blockedUntil
    )
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
    internal static PatientPenalty CreateAutomaticWarning(
        Guid patientId,
        Guid? appointmentId,
        string reason
    )
    {
        if (patientId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        return new PatientPenalty(patientId, appointmentId, PenaltyType.Warning, reason, null);
    }

    /// <summary>
    /// Creates a temporary block penalty that prevents the patient from booking until the specified date.
    /// </summary>
    /// <param name="blockedUntil">UTC date and time until which the block is in effect. Must be in the future.</param>
    /// <exception cref="DomainValidationException">Thrown when the patient ID is empty, the reason is blank, or the block date is not in the future.</exception>
    internal static PatientPenalty CreateAutomaticBlock(
        Guid patientId,
        string reason,
        DateTime blockedUntil,
        DateTime referenceTime
    )
    {
        if (patientId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);
        if (blockedUntil <= referenceTime)
            throw new DomainValidationException(DomainErrors.Validation.ValueMustBeInFuture);

        return new PatientPenalty(
            patientId,
            null,
            PenaltyType.TemporaryBlock,
            reason,
            blockedUntil
        );
    }

    /// <summary>
    /// Removes this penalty, marking it as no longer in effect.
    /// </summary>
    /// <exception cref="DomainValidationException">Thrown when the penalty has already been removed.</exception>
    public void Remove()
    {
        if (IsRemoved)
            throw new DomainValidationException(DomainErrors.Penalty.AlreadyRemoved);

        IsRemoved = true;
    }
}
