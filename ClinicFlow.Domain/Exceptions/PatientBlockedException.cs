namespace ClinicFlow.Domain.Exceptions;

public class PatientBlockedException(DateTime blockedUntil) : DomainException($"Patient is blocked from booking appointments until {blockedUntil}.")
{
    public DateTime BlockedUntil { get; } = blockedUntil;
}
