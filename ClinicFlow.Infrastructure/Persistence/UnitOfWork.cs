using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Persistence;

/// <summary>
/// Orchestrates the transaction boundary and publishes captured domain events.
/// </summary>
/// <remarks>
/// Events are captured and cleared before persistence to prevent re publication on retry.
/// If an explicit transaction is active, publication is deferred until the transaction commits,
/// ensuring consistency between persisted state and side effects.
/// Otherwise, events are published immediately after <see cref="SaveChangesAsync"/> completes.
/// </remarks>
public sealed class UnitOfWork(ApplicationDbContext dbContext, IPublisher publisher) : IUnitOfWork
{
    private readonly List<INotification> _pendingNotifications = [];

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEntities = dbContext
            .ChangeTracker.Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents.Count > 0)
            .ToList();

        var domainEvents = domainEntities.SelectMany(x => x.Entity.DomainEvents).ToList();

        foreach (var entity in domainEntities)
            entity.Entity.ClearDomainEvents();

        var result = await dbContext.SaveChangesAsync(cancellationToken);

        if (dbContext.Database.CurrentTransaction is not null)
        {
            foreach (var domainEvent in domainEvents)
                _pendingNotifications.Add(BuildNotification(domainEvent));
        }
        else
        {
            foreach (var domainEvent in domainEvents)
                await publisher.Publish(BuildNotification(domainEvent), cancellationToken);
        }

        return result;
    }

    public Task<TResult> ExecuteWithLockAsync<TResult>(
        Guid lockKey,
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default
    ) =>
        ExecuteWithLockCoreAsync(
            cancellationToken => AcquireLockAsync(ToStableLong(lockKey), cancellationToken),
            operation,
            cancellationToken
        );

    public Task<TResult> ExecuteWithLockAsync<TResult>(
        Guid lockKeyPart1,
        int lockKeyPart2,
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default
    ) =>
        ExecuteWithLockCoreAsync(
            cancellationToken =>
                AcquireLockAsync(ToStableInt(lockKeyPart1), lockKeyPart2, cancellationToken),
            operation,
            cancellationToken
        );

    private async Task<TResult> ExecuteWithLockCoreAsync<TResult>(
        Func<CancellationToken, Task> acquireLock,
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken
    )
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();

        var result = await strategy.ExecuteAsync(
            async (cancellationToken) =>
            {
                _pendingNotifications.Clear();

                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    cancellationToken
                );

                await acquireLock(cancellationToken);

                var operationResult = await operation(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                return operationResult;
            },
            cancellationToken
        );

        foreach (var notification in _pendingNotifications)
            await publisher.Publish(notification, cancellationToken);

        return result;
    }

    private Task<int> AcquireLockAsync(long key, CancellationToken cancellationToken) =>
        dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({key})",
            cancellationToken
        );

    private Task<int> AcquireLockAsync(int key1, int key2, CancellationToken cancellationToken) =>
        dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({key1}, {key2})",
            cancellationToken
        );

    private static INotification BuildNotification(object domainEvent)
    {
        var notificationType = typeof(DomainEventNotification<>).MakeGenericType(
            domainEvent.GetType()
        );
        return (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
    }

    private static long ToStableLong(Guid guid)
    {
        Span<byte> bytes = stackalloc byte[16];
        guid.TryWriteBytes(bytes);
        var high = BitConverter.ToInt64(bytes[..8]);
        var low = BitConverter.ToInt64(bytes[8..]);
        return high ^ low;
    }

    private static int ToStableInt(Guid guid)
    {
        var value = ToStableLong(guid);

        return unchecked((int)(value ^ (value >> 32)));
    }
}
