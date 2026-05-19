using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="Appointment"/> persistence operations.
/// </summary>
public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Appointment> Items, int TotalCount)> GetByDoctorIdPaginatedAsync(
        Guid doctorId,
        DateTime date,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    Task<(IReadOnlyList<Appointment> Items, int TotalCount)> GetByDateRangePaginatedAsync(
        DateTime startDate,
        DateTime endDate,
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

    Task<Appointment> CreateAsync(
        Appointment appointment,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Checks whether any patient under the specified user account has active future appointments.
    /// </summary>
    Task<bool> HasActiveAppointmentsForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Checks whether a scheduling conflict exists for the specified doctor, date, and time range.
    /// </summary>
    Task<bool> HasConflictAsync(
        Guid doctorId,
        DateTime scheduledDate,
        TimeRange timeRange,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Appointment>> GetFutureScheduledByDoctorIdAsync(
        Guid doctorId,
        DateTime referenceDate,
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
