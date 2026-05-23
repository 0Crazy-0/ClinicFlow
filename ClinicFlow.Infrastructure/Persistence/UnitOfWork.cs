using ClinicFlow.Application.Common.Models;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Interfaces;
using MediatR;

namespace ClinicFlow.Infrastructure.Persistence;

/// <summary>
/// Orchestrates the transaction boundary and publishes captured domain events.
/// </summary>
/// <remarks>
/// Events are captured and cleared before persistence to prevent re-publication on retry.
/// Handlers are invoked only after the transaction commits successfully, ensuring
/// consistency between persisted state and side effects.
/// </remarks>
public sealed class UnitOfWork(ApplicationDbContext dbContext, IPublisher publisher) : IUnitOfWork
{
    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEntities = dbContext
            .ChangeTracker.Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();

        var domainEvents = domainEntities.SelectMany(x => x.Entity.DomainEvents).ToList();

        foreach (var entity in domainEntities)
            entity.Entity.ClearDomainEvents();

        var result = await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in domainEvents)
        {
            var eventType = domainEvent.GetType();
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(eventType);
            var notification = Activator.CreateInstance(notificationType, domainEvent);

            if (notification is INotification mediatrNotification)
                await publisher.Publish(mediatrNotification, cancellationToken);
        }

        return result;
    }
}
