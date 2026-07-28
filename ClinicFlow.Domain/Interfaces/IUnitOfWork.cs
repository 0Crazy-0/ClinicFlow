namespace ClinicFlow.Domain.Interfaces;

/// <summary>
/// Abstraction for committing all pending changes within a single transactional boundary.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes <paramref name="operation"/> inside a transaction protected by a
    /// PostgreSQL advisory lock derived from <paramref name="lockKey"/>.
    /// </summary>
    /// <remarks>
    /// The execution strategy may retry <paramref name="operation"/> in full on transient failures.
    /// It must be safe to re-execute, meaning idempotent or free of non-transactional side effects
    /// such as external API calls. Domain events raised during the operation are captured and are
    /// not published until the transaction commits successfully.
    /// </remarks>
    Task<TResult> ExecuteWithLockAsync<TResult>(
        Guid lockKey,
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default
    );

    /// <inheritdoc cref="ExecuteWithLockAsync{TResult}(Guid, Func{CancellationToken, Task{TResult}}, CancellationToken)"/>
    Task<TResult> ExecuteWithLockAsync<TResult>(
        Guid lockKeyPart1,
        int lockKeyPart2,
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default
    );
}
