namespace ClinicFlow.Domain.Exceptions;

public class AppointmentCancellationUnauthorizedException(string message) : DomainException(message)
{
}
