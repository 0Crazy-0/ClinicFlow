using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Scheduling;

/// <summary>
/// Thrown when a doctor does not have an active schedule covering the requested day and time.
/// </summary>
public class DoctorNotAvailableException(Guid doctorId, DayOfWeek dayOfWeek) : DomainException($"Doctor {doctorId} is not available on {dayOfWeek}")
{
}
