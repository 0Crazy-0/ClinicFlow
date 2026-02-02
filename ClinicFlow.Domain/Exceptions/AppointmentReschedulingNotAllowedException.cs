namespace ClinicFlow.Domain.Exceptions;

public class AppointmentReschedulingNotAllowedException(string reason) : DomainException($"Cannot reschedule appointment: {reason}")
{
}
