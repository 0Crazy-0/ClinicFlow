using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides the repository implementation for <see cref="ClinicalFormTemplate"/> persistence operations.
/// </summary>
public sealed class ClinicalFormTemplateRepository(ApplicationDbContext dbContext)
    : IClinicalFormTemplateRepository
{
    public Task CreateAsync(
        ClinicalFormTemplate template,
        CancellationToken cancellationToken = default
    )
    {
        dbContext.ClinicalFormTemplates.Add(template);
        return Task.CompletedTask;
    }

    public async Task<ClinicalFormTemplate?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext.ClinicalFormTemplates.FirstOrDefaultAsync(
            c => c.Id == id,
            cancellationToken
        );

    public async Task<ClinicalFormTemplate?> GetByCodeAsync(
        string templateCode,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext.ClinicalFormTemplates.FirstOrDefaultAsync(
            c => c.Code == templateCode,
            cancellationToken
        );

    public async Task<bool> ExistsByCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    ) => await dbContext.ClinicalFormTemplates.AnyAsync(c => c.Code == code, cancellationToken);

    public async Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default
    ) => await dbContext.ClinicalFormTemplates.AnyAsync(c => c.Name == name, cancellationToken);

    public async Task<ClinicalFormTemplate?> GetByIdIncludingDeletedAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .ClinicalFormTemplates.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<bool> ExistsByNameExcludingAsync(
        string name,
        Guid excludeId,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext.ClinicalFormTemplates.AnyAsync(
            c => c.Name == name && c.Id != excludeId,
            cancellationToken
        );

    public async Task<IReadOnlyList<ClinicalFormTemplate>> GetAllActiveAsync(
        CancellationToken cancellationToken = default
    ) => await dbContext.ClinicalFormTemplates.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ClinicalFormTemplate>> GetAllIncludingDeletedAsync(
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .ClinicalFormTemplates.IgnoreQueryFilters()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}
