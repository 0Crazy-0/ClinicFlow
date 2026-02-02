namespace ClinicFlow.Domain.Exceptions;

public class InvalidAppointmentTypeException(string message) : DomainException(message)
{
}
