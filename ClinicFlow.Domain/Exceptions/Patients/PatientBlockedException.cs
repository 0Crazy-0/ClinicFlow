using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Patients;

/// <summary>
/// Thrown when a patient attempts to book an appointment while under a temporary block penalty.
/// </summary>
public class PatientBlockedException(DateTime blockedUntil) : DomainException($"Patient is blocked from booking appointments until {blockedUntil}.")
{
    /// <summary>
    /// UTC date and time until which the patient is blocked.
    /// </summary>
    public DateTime BlockedUntil { get; } = blockedUntil;
}
