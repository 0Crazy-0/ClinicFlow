using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

/// <summary>
/// Thrown when an appointment cannot be rescheduled due to policy or status constraints.
/// </summary>
public class AppointmentReschedulingNotAllowedException(string reason) : DomainException($"Cannot reschedule appointment: {reason}")
{
}
