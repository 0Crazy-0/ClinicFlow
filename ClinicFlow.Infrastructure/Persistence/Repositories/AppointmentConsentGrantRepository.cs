using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Persistence.Repositories;

public sealed class AppointmentConsentGrantRepository(ApplicationDbContext dbContext)
    : IAppointmentConsentGrantRepository
{
    public Task CreateAsync(
        AppointmentConsentGrant grant,
        CancellationToken cancellationToken = default
    )
    {
        if (dbContext.Entry(grant).State is EntityState.Detached)
            dbContext.AppointmentConsentGrants.Add(grant);

        return Task.CompletedTask;
    }

    public async Task<AppointmentConsentGrant?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext.AppointmentConsentGrants.FirstOrDefaultAsync(
            g => g.Id == id,
            cancellationToken
        );

    public async Task<bool> HasEffectiveCategoryGrantAsync(
        Guid patientId,
        Guid recipientMembershipId,
        ProtectedCategory protectedCareCategory,
        DateTime referenceTime,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext.AppointmentConsentGrants.AnyAsync(
            g =>
                g.PatientId == patientId
                && g.RecipientMembershipId == recipientMembershipId
                && g.Scope == AppointmentConsentScope.Category
                && g.ProtectedCareCategory == protectedCareCategory
                && g.RevokedAt == null
                && referenceTime < g.ExpiresAt,
            cancellationToken
        );

    public async Task<bool> HasEffectiveAppointmentGrantAsync(
        Guid patientId,
        Guid recipientMembershipId,
        Guid appointmentId,
        DateTime referenceTime,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext.AppointmentConsentGrants.AnyAsync(
            g =>
                g.PatientId == patientId
                && g.RecipientMembershipId == recipientMembershipId
                && g.Scope == AppointmentConsentScope.SpecificAppointment
                && g.AppointmentId == appointmentId
                && g.RevokedAt == null
                && referenceTime < g.ExpiresAt,
            cancellationToken
        );

    public async Task<IReadOnlyCollection<ProtectedCategory>> GetEffectiveCategoriesAsync(
        Guid patientId,
        Guid recipientMembershipId,
        DateTime referenceTime,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .AppointmentConsentGrants.AsNoTracking()
            .Where(g =>
                g.PatientId == patientId
                && g.RecipientMembershipId == recipientMembershipId
                && g.Scope == AppointmentConsentScope.Category
                && g.ProtectedCareCategory != null
                && g.RevokedAt == null
                && referenceTime < g.ExpiresAt
            )
            .Select(g => g.ProtectedCareCategory!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Guid>> GetEffectiveAppointmentIdsAsync(
        Guid patientId,
        Guid recipientMembershipId,
        DateTime referenceTime,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .AppointmentConsentGrants.AsNoTracking()
            .Where(g =>
                g.PatientId == patientId
                && g.RecipientMembershipId == recipientMembershipId
                && g.Scope == AppointmentConsentScope.SpecificAppointment
                && g.AppointmentId != null
                && g.RevokedAt == null
                && referenceTime < g.ExpiresAt
            )
            .Select(g => g.AppointmentId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);
}
