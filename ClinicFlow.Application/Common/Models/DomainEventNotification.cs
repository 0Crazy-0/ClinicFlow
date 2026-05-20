using ClinicFlow.Domain.Common;
using MediatR;

namespace ClinicFlow.Application.Common.Models;

/// <summary>
/// Adapts a domain event to a MediatR notification for asynchronous dispatching.
/// </summary>
/// <param name="domainEvent">The domain event instance being wrapped.</param>
public class DomainEventNotification<TDomainEvent>(TDomainEvent domainEvent) : INotification
    where TDomainEvent : IDomainEvent
{
    public TDomainEvent DomainEvent { get; } = domainEvent;
}
