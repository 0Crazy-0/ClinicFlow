using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Patients;

/// <summary>
/// Thrown when a patient attempts to book an appointment while under a temporary block penalty.
/// </summary>
public class PatientBlockedException(string errorCode, DateTime blockedUntil)
    : DomainException(errorCode)
{
    public DateTime BlockedUntil { get; } = blockedUntil;
}
