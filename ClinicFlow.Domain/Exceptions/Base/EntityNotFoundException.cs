namespace ClinicFlow.Domain.Exceptions.Base;

public class EntityNotFoundException(string errorCode, string entityName, object id)
    : DomainException(errorCode)
{
    public string EntityName { get; } = entityName;
    public object Id { get; } = id;
}
