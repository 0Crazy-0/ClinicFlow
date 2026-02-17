namespace ClinicFlow.Domain.Exceptions;

public class InvalidAppointmentException(string message) : DomainException(message)
{
}
