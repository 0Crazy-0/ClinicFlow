using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for <see cref="ClinicalFormTemplate"/> persistence operations.
/// </summary>
public sealed class ClinicalFormTemplateRepository(ApplicationDbContext dbContext)
    : IClinicalFormTemplateRepository
{
    /// <inheritdoc />
    public async Task<ClinicalFormTemplate?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext.ClinicalFormTemplates.FirstOrDefaultAsync(
            c => c.Id == id,
            cancellationToken
        );

    /// <inheritdoc />
    public async Task<ClinicalFormTemplate?> GetByCodeAsync(
        string templateCode,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext.ClinicalFormTemplates.FirstOrDefaultAsync(
            c => c.Code == templateCode,
            cancellationToken
        );

    /// <inheritdoc />
    public async Task<bool> ExistsByCodeAsync(
        string code,
        CancellationToken cancellationToken = default
    ) => await dbContext.ClinicalFormTemplates.AnyAsync(c => c.Code == code, cancellationToken);

    /// <inheritdoc />
    public async Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default
    ) => await dbContext.ClinicalFormTemplates.AnyAsync(c => c.Name == name, cancellationToken);

    /// <inheritdoc />
    public async Task<ClinicalFormTemplate?> GetByIdIncludingDeletedAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .ClinicalFormTemplates.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<bool> ExistsByNameExcludingAsync(
        string name,
        Guid excludeId,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext.ClinicalFormTemplates.AnyAsync(
            c => c.Name == name && c.Id != excludeId,
            cancellationToken
        );

    /// <inheritdoc />
    public async Task<IReadOnlyList<ClinicalFormTemplate>> GetAllActiveAsync(
        CancellationToken cancellationToken = default
    ) => await dbContext.ClinicalFormTemplates.AsNoTracking().ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<ClinicalFormTemplate>> GetAllIncludingDeletedAsync(
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .ClinicalFormTemplates.IgnoreQueryFilters()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task CreateAsync(
        ClinicalFormTemplate template,
        CancellationToken cancellationToken = default
    ) => await dbContext.ClinicalFormTemplates.AddAsync(template, cancellationToken);
}
