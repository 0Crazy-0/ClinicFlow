namespace ClinicFlow.Domain.Exceptions.Base;

public class BusinessRuleValidationException(string message) : DomainException(message)
{
}
