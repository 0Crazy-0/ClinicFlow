namespace ClinicFlow.Domain.Exceptions.Base;

/// <summary>
/// Thrown when a requested entity cannot be found by its identifier.
/// </summary>
public class EntityNotFoundException(string entityName, object id) : DomainException($"Entity '{entityName}' with id '{id}' was not found.")
{
}
