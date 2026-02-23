namespace ClinicFlow.Domain.Common;

/// <summary>
/// Base class for all domain entities, providing identity, auditing, soft-delete, and domain event support.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// UTC timestamp of when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// UTC timestamp of the last modification.
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Indicates whether the entity has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; private set; }

    private readonly IList<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Domain events raised by this entity, pending dispatch.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }

    /// <summary>
    /// Registers a domain event to be dispatched after persistence.
    /// </summary>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes all pending domain events.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Marks the entity as soft-deleted and updates the modification timestamp.
    /// </summary>
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}