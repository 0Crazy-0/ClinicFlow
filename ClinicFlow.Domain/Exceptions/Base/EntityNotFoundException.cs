namespace ClinicFlow.Domain.Exceptions.Base;

/// <summary>
/// Thrown when a requested entity cannot be found by its identifier.
/// </summary>
public class EntityNotFoundException(string errorCode, string entityName, object id) : DomainException(errorCode)
{
    public string EntityName { get; } = entityName;
    public object Id { get; } = id;
}
