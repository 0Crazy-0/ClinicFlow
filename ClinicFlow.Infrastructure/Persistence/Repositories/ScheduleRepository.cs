using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Provides the repository implementation for <see cref="Schedule"/> persistence operations.
/// </summary>
public sealed class ScheduleRepository(ApplicationDbContext dbContext) : IScheduleRepository
{
    public Task CreateAsync(Schedule schedule, CancellationToken cancellationToken = default)
    {
        dbContext.Schedules.Add(schedule);
        return Task.CompletedTask;
    }

    public Task CreateRangeAsync(
        IReadOnlyList<Schedule> schedules,
        CancellationToken cancellationToken = default
    )
    {
        dbContext.Schedules.AddRange(schedules);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<Schedule?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) => await dbContext.Schedules.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<Schedule?> GetActiveByDoctorAndDayAsync(
        Guid doctorId,
        DayOfWeek day,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext.Schedules.FirstOrDefaultAsync(
            s => s.DoctorId == doctorId && s.DayOfWeek == day && s.IsActive,
            cancellationToken
        );

    /// <inheritdoc />
    public async Task<IReadOnlyList<Schedule>> GetByDoctorIdAsync(
        Guid doctorId,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .Schedules.AsNoTracking()
            .Where(s => s.DoctorId == doctorId)
            .OrderBy(s => s.DayOfWeek)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Schedule>> GetActiveByDoctorIdAsync(
        Guid doctorId,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .Schedules.Where(s => s.DoctorId == doctorId && s.IsActive)
            .OrderBy(s => s.DayOfWeek)
            .ToListAsync(cancellationToken);
}
