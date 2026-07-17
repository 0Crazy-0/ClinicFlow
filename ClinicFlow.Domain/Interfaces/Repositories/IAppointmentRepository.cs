using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="Appointment"/> persistence operations.
/// </summary>
public interface IAppointmentRepository
{
    Task CreateAsync(Appointment appointment, CancellationToken cancellationToken = default);

    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Appointment> Items, int TotalCount)> GetByDoctorIdAndDateAsync(
        Guid doctorId,
        DateOnly date,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<(IReadOnlyList<Appointment> Items, int TotalCount)> GetByDateRangePaginatedAsync(
        DateOnly startDate,
        DateOnly endDate,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<(IReadOnlyList<Appointment> Items, int TotalCount)> GetByPatientIdPaginatedAsync(
        Guid patientId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<bool> HasActiveAppointmentsForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<bool> HasConflictAsync(
        Guid doctorId,
        DateOnly scheduledDate,
        TimeRange timeRange,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Appointment>> GetFutureScheduledByDoctorIdAsync(
        Guid doctorId,
        DateOnly referenceDate,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves all appointments in RequiresReassignment status whose scheduled time has passed.
    /// </summary>
    Task<IReadOnlyList<Appointment>> GetExpiredDisplacedAppointmentsAsync(
        DateTime referenceTime,
        CancellationToken cancellationToken = default
    );
}
