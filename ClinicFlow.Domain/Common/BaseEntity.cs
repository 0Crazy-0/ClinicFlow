namespace ClinicFlow.Domain.Common;

/// <summary>
/// Base class for all domain entities, providing identity, auditing, soft-delete, and domain event support.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the strictly increasing sequence value assigned by the database on insertion.
    /// </summary>
    /// <remarks>
    /// Guid v7 identifiers encode a millisecond timestamp, so entities created within the
    /// same millisecond can be ordered arbitrarily when sorting by <see cref="Id"/>. This
    /// property provides a database generated, gapless ordering key for deterministic
    /// sorting and stable pagination.
    /// </remarks>
    public long SequenceNumber { get; }

    /// <summary>
    /// Represents the row's concurrency token, mapped to PostgreSQL's system column "xmin".
    /// </summary>
    public uint Version { get; }
    private readonly IList<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Domain events raised by this entity, pending dispatch.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected BaseEntity()
    {
        Id = Guid.CreateVersion7();
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
