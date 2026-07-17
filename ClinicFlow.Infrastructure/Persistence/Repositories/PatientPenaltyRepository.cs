using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides the repository implementation for <see cref="PatientPenalty"/> persistence operations.
/// </summary>
public sealed class PatientPenaltyRepository(ApplicationDbContext dbContext)
    : IPatientPenaltyRepository
{
    public Task CreateAsync(PatientPenalty penalty, CancellationToken cancellationToken = default)
    {
        dbContext.PatientPenalties.Add(penalty);
        return Task.CompletedTask;
    }

    public Task CreateRangeAsync(
        IEnumerable<PatientPenalty> penalties,
        CancellationToken cancellationToken = default
    )
    {
        dbContext.PatientPenalties.AddRange(penalties);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<PatientPenalty?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) => await dbContext.PatientPenalties.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<PatientPenalty>> GetHistoryByPatientIdAsync(
        Guid patientId,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .PatientPenalties.AsNoTracking()
            .Where(p => p.PatientId == patientId)
            .OrderByDescending(p => p.SequenceNumber)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<(
        IReadOnlyList<PatientPenalty> Items,
        int TotalCount
    )> GetHistoryByPatientIdPaginatedAsync(
        Guid patientId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext.PatientPenalties.AsNoTracking().Where(p => p.PatientId == patientId);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(p => p.SequenceNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<PatientPenalty>> GetActiveBlocksByPatientIdAsync(
        Guid patientId,
        DateOnly referenceDate,
        CancellationToken cancellationToken = default
    ) =>
        await ActiveBlocksQuery(referenceDate)
            .Where(p => p.PatientId == patientId)
            .OrderBy(p => p.BlockedUntil)
            .ThenBy(p => p.SequenceNumber)
            .ToListAsync(cancellationToken);

    public async Task<(
        IReadOnlyList<PatientPenalty> Items,
        int TotalCount
    )> GetActiveBlocksPaginatedAsync(
        DateOnly referenceDate,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var query = ActiveBlocksQuery(referenceDate);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(p => p.BlockedUntil)
            .ThenBy(p => p.SequenceNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(
        IReadOnlyList<PatientPenalty> Items,
        int TotalCount
    )> GetActiveWarningsPaginatedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext
            .PatientPenalties.AsNoTracking()
            .Where(p => !p.IsRemoved && p.Type == PenaltyType.Warning);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(p => p.SequenceNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    private IQueryable<PatientPenalty> ActiveBlocksQuery(DateOnly referenceDate) =>
        dbContext
            .PatientPenalties.AsNoTracking()
            .Where(p =>
                !p.IsRemoved
                && p.Type == PenaltyType.TemporaryBlock
                && p.BlockedUntil > referenceDate
            );
}
