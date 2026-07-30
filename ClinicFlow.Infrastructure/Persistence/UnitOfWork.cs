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

    public async Task<TResult> ExecuteWithLockAsync<TResult>(
        Guid lockKey,
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default
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

                await AcquireLockAsync(ToStableLong(lockKey), cancellationToken);

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
}
