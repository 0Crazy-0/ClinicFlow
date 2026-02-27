namespace ClinicFlow.Domain.Interfaces;

/// <summary>
/// Abstraction for committing all pending changes within a single transactional boundary.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
