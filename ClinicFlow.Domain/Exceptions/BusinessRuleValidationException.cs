namespace ClinicFlow.Domain.Exceptions;

public class BusinessRuleValidationException(string message) : DomainException(message)
{
}
