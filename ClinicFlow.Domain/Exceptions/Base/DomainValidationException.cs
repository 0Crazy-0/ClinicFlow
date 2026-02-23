namespace ClinicFlow.Domain.Exceptions.Base;

/// <summary>
/// Thrown when an entity fails input or state validation within the domain.
/// </summary>
public class DomainValidationException(string message) : DomainException(message)
{
}
