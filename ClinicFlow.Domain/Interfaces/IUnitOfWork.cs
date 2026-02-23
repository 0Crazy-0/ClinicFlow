namespace ClinicFlow.Domain.Interfaces;

/// <summary>
/// Abstraction for committing all pending changes within a single transactional boundary.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes to the underlying data store.
    /// </summary>
    /// <returns>The number of state entries written.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
