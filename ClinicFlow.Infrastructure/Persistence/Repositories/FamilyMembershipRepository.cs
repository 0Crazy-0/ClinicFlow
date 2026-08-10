using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides the repository implementation for <see cref="FamilyMembership"/> persistence operations.
/// </summary>
public sealed class FamilyMembershipRepository(ApplicationDbContext dbContext)
    : IFamilyMembershipRepository
{
    public Task CreateAsync(
        FamilyMembership familyMembership,
        CancellationToken cancellationToken = default
    )
    {
        if (dbContext.Entry(familyMembership).State is EntityState.Detached)
            dbContext.FamilyMemberships.Add(familyMembership);

        return Task.CompletedTask;
    }

    public async Task<bool> HasActiveSelfMembershipByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext.FamilyMemberships.AnyAsync(
            m =>
                m.UserId == userId
                && m.Status == FamilyMembershipStatus.Active
                && m.Role == PatientRelationship.Self,
            cancellationToken
        );

    public async Task<bool> HasActiveSelfMembershipByPatientIdAsync(
        Guid patientId,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext.FamilyMemberships.AnyAsync(
            m =>
                m.PatientId == patientId
                && m.Status == FamilyMembershipStatus.Active
                && m.Role == PatientRelationship.Self,
            cancellationToken
        );

    /// <inheritdoc />
    public async Task<bool> HasActiveMembershipAsync(
        Guid userId,
        Guid patientId,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext.FamilyMemberships.AnyAsync(
            m =>
                m.UserId == userId
                && m.PatientId == patientId
                && m.Status == FamilyMembershipStatus.Active,
            cancellationToken
        );

    public async Task<int> CountActiveFamilyMembersAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .FamilyMemberships.AsNoTracking()
            .CountAsync(
                m =>
                    m.UserId == userId
                    && m.Status == FamilyMembershipStatus.Active
                    && m.Role != PatientRelationship.Self,
                cancellationToken
            );

    /// <inheritdoc />
    public async Task<IReadOnlyList<FamilyMembership>> GetHistoryByPatientIdAsync(
        Guid patientId,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .FamilyMemberships.AsNoTracking()
            .Where(m => m.PatientId == patientId)
            .OrderByDescending(m => m.StartedAt)
            .ToListAsync(cancellationToken);
}
