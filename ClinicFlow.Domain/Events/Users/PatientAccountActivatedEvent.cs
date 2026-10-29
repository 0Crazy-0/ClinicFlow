using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.Events.Users;

/// <summary>
/// Occurs when a pending patient account is activated with its initial password.
/// </summary>
public sealed record PatientAccountActivatedEvent(Guid UserId, DateTime ActivatedAt) : IDomainEvent;
