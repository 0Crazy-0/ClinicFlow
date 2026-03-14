namespace ClinicFlow.Domain.Exceptions.Base;

/// <summary>
/// Abstract base exception for all domain-layer errors.
/// </summary>
public abstract class DomainException(string errorCode) : Exception(errorCode)
{
}
