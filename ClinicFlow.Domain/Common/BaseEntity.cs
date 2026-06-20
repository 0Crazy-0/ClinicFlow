namespace ClinicFlow.Domain.Common;

/// <summary>
/// Base class for all domain entities, providing identity, auditing, soft-delete, and domain event support.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; private set; }
    private readonly IList<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Domain events raised by this entity, pending dispatch.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Registers a domain event to be dispatched after persistence.
    /// </summary>
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>
    /// Removes all pending domain events.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
