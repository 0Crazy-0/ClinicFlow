namespace ClinicFlow.Domain.Exceptions;

public class InvalidTimeRangeException(string message) : DomainException(message)
{
}
