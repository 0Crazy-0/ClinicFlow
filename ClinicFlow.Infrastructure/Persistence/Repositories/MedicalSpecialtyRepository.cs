using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides the repository implementation for <see cref="MedicalSpecialty"/> persistence operations.
/// </summary>
public sealed class MedicalSpecialtyRepository(ApplicationDbContext dbContext)
    : IMedicalSpecialtyRepository
{
    public Task CreateAsync(
        MedicalSpecialty specialty,
        CancellationToken cancellationToken = default
    )
    {
        dbContext.MedicalSpecialties.Add(specialty);
        return Task.CompletedTask;
    }

    public async Task<MedicalSpecialty?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) => await dbContext.MedicalSpecialties.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<MedicalSpecialty?> GetByIdIncludingDeletedAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .MedicalSpecialties.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default
    ) => await dbContext.MedicalSpecialties.AnyAsync(s => s.Name == name, cancellationToken);

    public async Task<bool> ExistsByNameExcludingAsync(
        string name,
        Guid excludeId,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext.MedicalSpecialties.AnyAsync(
            s => s.Name == name && s.Id != excludeId,
            cancellationToken
        );

    public async Task<IReadOnlyList<MedicalSpecialty>> GetAllActiveAsync(
        CancellationToken cancellationToken = default
    ) => await dbContext.MedicalSpecialties.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<MedicalSpecialty>> GetAllIncludingDeletedAsync(
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .MedicalSpecialties.AsNoTracking()
            .IgnoreQueryFilters()
            .ToListAsync(cancellationToken);
}
