using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Patients;

public class PatientBlockedException(string errorCode, DateOnly blockedUntil)
    : DomainException(errorCode)
{
    public DateOnly BlockedUntil { get; } = blockedUntil;
}
