namespace ClinicFlow.Domain.Exceptions;

public class EntityNotFoundException(string entityName, object id) 
    : DomainException($"Entity '{entityName}' with id '{id}' was not found.")
{
}
