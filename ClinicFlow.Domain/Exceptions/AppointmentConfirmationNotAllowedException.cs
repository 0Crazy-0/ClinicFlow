namespace ClinicFlow.Domain.Exceptions;

public class AppointmentConfirmationNotAllowedException(string reason) : DomainException($"Cannot confirm appointment: {reason}")
{
}
