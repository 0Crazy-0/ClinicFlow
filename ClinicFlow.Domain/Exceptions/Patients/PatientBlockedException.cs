using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Patients;

public class PatientBlockedException(string errorCode, DateTime blockedUntil)
    : DomainException(errorCode)
{
    public DateTime BlockedUntil { get; } = blockedUntil;
}
