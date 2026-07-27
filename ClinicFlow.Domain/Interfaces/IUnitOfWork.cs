namespace ClinicFlow.Domain.Interfaces;

/// <summary>
/// Abstraction for committing all pending changes within a single transactional boundary.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<TResult> ExecuteWithLockAsync<TResult>(
        Guid lockKey,
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default
    );

    Task<TResult> ExecuteWithLockAsync<TResult>(
        Guid lockKeyPart1,
        int lockKeyPart2,
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default
    );
}
