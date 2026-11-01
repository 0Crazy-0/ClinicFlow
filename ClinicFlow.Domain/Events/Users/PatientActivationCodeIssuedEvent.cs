using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.Events.Users;

/// <remarks>
/// The payload never carries the plaintext code or its hash.
/// </remarks>
public sealed record PatientActivationCodeIssuedEvent(
    Guid UserId,
    Guid IssuedByUserId,
    DateTime IssuedAt,
    DateTime ExpiresAt
) : IDomainEvent;
