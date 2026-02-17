namespace ClinicFlow.Domain.Exceptions.Base;

public abstract class DomainException(string message) : Exception(message)
{
}
