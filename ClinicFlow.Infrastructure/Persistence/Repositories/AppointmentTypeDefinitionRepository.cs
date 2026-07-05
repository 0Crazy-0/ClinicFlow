using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for <see cref="AppointmentTypeDefinition"/> persistence operations.
/// </summary>
public sealed class AppointmentTypeDefinitionRepository(ApplicationDbContext dbContext)
    : IAppointmentTypeDefinitionRepository
{
    public async Task<AppointmentTypeDefinition?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .AppointmentTypes.Include(a => a.RequiredTemplates)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyList<AppointmentTypeDefinition>> GetAllActiveAsync(
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .AppointmentTypes.AsNoTracking()
            .Include(a => a.RequiredTemplates)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AppointmentTypeDefinition>> GetByCategoryAsync(
        AppointmentCategory category,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .AppointmentTypes.AsNoTracking()
            .Include(a => a.RequiredTemplates)
            .Where(a => a.Category == category)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AppointmentTypeDefinition>> GetEligibleByAgeAsync(
        int patientAgeInYears,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .AppointmentTypes.AsNoTracking()
            .Include(a => a.RequiredTemplates)
            .Where(a =>
                (a.AgePolicy.MinimumAge == null || patientAgeInYears >= a.AgePolicy.MinimumAge)
                && (a.AgePolicy.MaximumAge == null || patientAgeInYears <= a.AgePolicy.MaximumAge)
            )
            .ToListAsync(cancellationToken);

    public async Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default
    ) => await dbContext.AppointmentTypes.AnyAsync(a => a.Name == name, cancellationToken);

    public async Task<AppointmentTypeDefinition?> GetByIdIncludingDeletedAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .AppointmentTypes.IgnoreQueryFilters()
            .Include(a => a.RequiredTemplates)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<bool> ExistsByNameExcludingAsync(
        string name,
        Guid excludeId,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext.AppointmentTypes.AnyAsync(
            a => a.Name == name && a.Id != excludeId,
            cancellationToken
        );

    public async Task CreateAsync(
        AppointmentTypeDefinition appointmentType,
        CancellationToken cancellationToken = default
    ) => await dbContext.AppointmentTypes.AddAsync(appointmentType, cancellationToken);
}
