using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Patients;

public class PatientBlockedException(DateTime blockedUntil) : DomainException($"Patient is blocked from booking appointments until {blockedUntil}.")
{
    public DateTime BlockedUntil { get; } = blockedUntil;
}
