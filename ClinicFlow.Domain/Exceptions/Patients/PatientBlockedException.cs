using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Patients;

/// <summary>
/// Thrown when a patient attempts to book an appointment while under a temporary block penalty.
/// </summary>
public class PatientBlockedException(DateTime blockedUntil) : DomainException($"Patient is blocked from booking appointments until {blockedUntil}.")
{
    public DateTime BlockedUntil { get; } = blockedUntil;
}
