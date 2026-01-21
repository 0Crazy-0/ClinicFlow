namespace ClinicFlow.Domain.Exceptions
{
    public class PatientBlockedException(DateTime blockedUntil)
     : DomainException($"The patient is blocked until {blockedUntil:yyyy-MM-dd} due to late cancellations")
    {
    }
}