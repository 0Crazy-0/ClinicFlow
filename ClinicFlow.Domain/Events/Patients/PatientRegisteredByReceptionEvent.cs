using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.Events.Patients;

/// <summary>
/// Occurs when reception registers a new patient profile: an adult with a pending account, a dependent minor under a guardian account, or an independent minor with a pending account.
/// </summary>
public sealed record PatientRegisteredByReceptionEvent(
    Guid PatientId,
    Guid ReceptionistUserId,
    DateTime RegisteredAt
) : IDomainEvent;
