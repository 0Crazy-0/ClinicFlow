using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides the repository implementation for <see cref="MedicalRecordConsentGrant"/> persistence operations.
/// </summary>
public sealed class MedicalRecordConsentGrantRepository(ApplicationDbContext dbContext)
    : IMedicalRecordConsentGrantRepository
{
    public Task CreateAsync(
        MedicalRecordConsentGrant grant,
        CancellationToken cancellationToken = default
    )
    {
        if (dbContext.Entry(grant).State is EntityState.Detached)
            dbContext.MedicalRecordConsentGrants.Add(grant);

        return Task.CompletedTask;
    }

    public async Task<MedicalRecordConsentGrant?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext.MedicalRecordConsentGrants.FirstOrDefaultAsync(
            g => g.Id == id,
            cancellationToken
        );

    public async Task<bool> HasEffectiveGrantAsync(
        Guid medicalRecordId,
        Guid recipientMembershipId,
        DateTime referenceTime,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext.MedicalRecordConsentGrants.AnyAsync(
            g =>
                g.MedicalRecordId == medicalRecordId
                && g.RecipientMembershipId == recipientMembershipId
                && g.RevokedAt == null
                && referenceTime < g.ExpiresAt,
            cancellationToken
        );

    public async Task<IReadOnlyList<Guid>> GetEffectiveRecordIdsAsync(
        Guid patientId,
        Guid recipientMembershipId,
        DateTime referenceTime,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .MedicalRecordConsentGrants.AsNoTracking()
            .Where(g =>
                g.PatientId == patientId
                && g.RecipientMembershipId == recipientMembershipId
                && g.RevokedAt == null
                && referenceTime < g.ExpiresAt
            )
            .Select(g => g.MedicalRecordId)
            .ToListAsync(cancellationToken);

    public async Task<(
        IReadOnlyList<MedicalRecordConsentGrant> Items,
        int TotalCount
    )> GetByPatientIdPaginatedAsync(
        Guid patientId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext
            .MedicalRecordConsentGrants.AsNoTracking()
            .Where(g => g.PatientId == patientId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(g => g.SequenceNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
