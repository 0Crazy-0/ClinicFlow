using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Scheduling;

public class DoctorNotAvailableException(Guid doctorId, DayOfWeek dayOfWeek)
    : DomainException($"Doctor {doctorId} is not available on {dayOfWeek}")
{
}
