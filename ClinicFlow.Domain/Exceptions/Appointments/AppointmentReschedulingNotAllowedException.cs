using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

public class AppointmentReschedulingNotAllowedException(string reason) : DomainException($"Cannot reschedule appointment: {reason}")
{
}
