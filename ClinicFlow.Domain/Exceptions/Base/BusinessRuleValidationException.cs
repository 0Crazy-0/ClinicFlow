namespace ClinicFlow.Domain.Exceptions.Base;

/// <summary>
/// Thrown when a domain business rule is violated.
/// </summary>
public class BusinessRuleValidationException(string message) : DomainException(message)
{
}
