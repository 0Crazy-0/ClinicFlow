namespace ClinicFlow.Domain.Exceptions.Base;

public class DomainValidationException(string message) : DomainException(message)
{
}
