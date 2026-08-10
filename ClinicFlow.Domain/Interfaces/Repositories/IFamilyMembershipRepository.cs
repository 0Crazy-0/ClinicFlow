using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="FamilyMembership"/> persistence operations.
/// </summary>
public interface IFamilyMembershipRepository
{
    Task CreateAsync(
        FamilyMembership familyMembership,
        CancellationToken cancellationToken = default
    );

    Task<bool> HasActiveSelfMembershipByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<bool> HasActiveSelfMembershipByPatientIdAsync(
        Guid patientId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Checks if an active membership exists between a specific user account and patient profile.
    /// </summary>
    Task<bool> HasActiveMembershipAsync(
        Guid userId,
        Guid patientId,
        CancellationToken cancellationToken = default
    );

    Task<int> CountActiveFamilyMembersAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves all historical memberships for a patient profile across all statuses.
    /// </summary>
    Task<IReadOnlyList<FamilyMembership>> GetHistoryByPatientIdAsync(
        Guid patientId,
        CancellationToken cancellationToken = default
    );
}
