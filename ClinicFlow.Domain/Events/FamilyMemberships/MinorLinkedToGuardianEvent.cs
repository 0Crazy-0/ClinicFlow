using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Events.FamilyMemberships;

/// <summary>
/// Occurs when a minor patient is linked to a guardian account through a family membership.
/// </summary>
public sealed record MinorLinkedToGuardianEvent(
    Guid MinorPatientId,
    Guid GuardianUserId,
    PatientRelationship Relationship,
    DateTime LinkedAt
) : IDomainEvent;
