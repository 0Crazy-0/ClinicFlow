namespace ClinicFlow.Domain.Exceptions;

public class DoctorNotAvailableException(Guid doctorId, DayOfWeek dayOfWeek)
    : DomainException($"Doctor {doctorId} is not available on {dayOfWeek}")
{
}
