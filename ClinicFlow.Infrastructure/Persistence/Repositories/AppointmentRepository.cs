using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Interfaces.Repositories;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ClinicFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for <see cref="Appointment"/> persistence operations.
/// </summary>
public sealed class AppointmentRepository(ApplicationDbContext dbContext, TimeProvider timeProvider)
    : IAppointmentRepository
{
    public async Task<Appointment?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    ) => await dbContext.Appointments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<Appointment> Items, int TotalCount)> GetByDoctorIdAndDateAsync(
        Guid doctorId,
        DateOnly date,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext
            .Appointments.AsNoTracking()
            .Where(a => a.DoctorId == doctorId && a.ScheduledDate == date);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(a => a.TimeRange.Start)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(
        IReadOnlyList<Appointment> Items,
        int TotalCount
    )> GetByDateRangePaginatedAsync(
        DateOnly startDate,
        DateOnly endDate,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext
            .Appointments.AsNoTracking()
            .Where(a => a.ScheduledDate >= startDate && a.ScheduledDate <= endDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(a => a.ScheduledDate)
            .ThenBy(a => a.TimeRange.Start)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(
        IReadOnlyList<Appointment> Items,
        int TotalCount
    )> GetByPatientIdPaginatedAsync(
        Guid patientId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var query = dbContext.Appointments.AsNoTracking().Where(a => a.PatientId == patientId);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(a => a.ScheduledDate)
            .ThenBy(a => a.TimeRange.Start)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task CreateAsync(
        Appointment appointment,
        CancellationToken cancellationToken = default
    ) => await dbContext.Appointments.AddAsync(appointment, cancellationToken);

    public async Task<bool> HasActiveAppointmentsForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var nowTime = TimeOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        return await dbContext.Appointments.AnyAsync(
            a =>
                dbContext
                    .Patients.Where(p => p.UserId == userId)
                    .Select(p => p.Id)
                    .Contains(a.PatientId)
                && (
                    a.Status == AppointmentStatus.Scheduled
                    || a.Status == AppointmentStatus.CheckedIn
                    || a.Status == AppointmentStatus.RequiresReassignment
                )
                && (
                    a.ScheduledDate > today
                    || (a.ScheduledDate == today && a.TimeRange.Start > nowTime)
                ),
            cancellationToken
        );
    }

    public async Task<bool> HasConflictAsync(
        Guid doctorId,
        DateOnly scheduledDate,
        TimeRange timeRange,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext.Appointments.AnyAsync(
            a =>
                a.DoctorId == doctorId
                && a.ScheduledDate == scheduledDate
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.LateCancellation
                && a.Status != AppointmentStatus.RequiresReassignment
                && a.TimeRange.Start < timeRange.End
                && timeRange.Start < a.TimeRange.End,
            cancellationToken
        );

    public async Task<IReadOnlyList<Appointment>> GetFutureScheduledByDoctorIdAsync(
        Guid doctorId,
        DateOnly referenceDate,
        CancellationToken cancellationToken = default
    ) =>
        await dbContext
            .Appointments.Where(a =>
                a.DoctorId == doctorId
                && a.ScheduledDate >= referenceDate
                && a.Status == AppointmentStatus.Scheduled
            )
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Appointment>> GetExpiredDisplacedAppointmentsAsync(
        DateTime referenceTime,
        CancellationToken cancellationToken = default
    )
    {
        var referenceDate = DateOnly.FromDateTime(referenceTime);
        var referenceTimeOnly = TimeOnly.FromDateTime(referenceTime);

        return await dbContext
            .Appointments.Where(a =>
                a.Status == AppointmentStatus.RequiresReassignment
                && (
                    a.ScheduledDate < referenceDate
                    || (a.ScheduledDate == referenceDate && a.TimeRange.Start < referenceTimeOnly)
                )
            )
            .ToListAsync(cancellationToken);
    }
}
