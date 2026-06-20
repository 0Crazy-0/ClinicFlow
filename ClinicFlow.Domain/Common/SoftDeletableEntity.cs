namespace ClinicFlow.Domain.Common;

/// <summary>
/// Provides soft-deletion capabilities for domain entities.
/// </summary>
public abstract class SoftDeletableEntity : BaseEntity
{
    /// <summary>
    /// Gets a value indicating whether the entity has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; private set; }

    protected void MarkAsDeleted() => IsDeleted = true;

    protected void UndoDeletion() => IsDeleted = false;
}
