using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="Appointment"/> persistence operations.
/// </summary>
public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Appointment>> GetByDoctorIdAsync(
        Guid doctorId,
        DateTime date,
        CancellationToken ct = default
    );

    Task<IReadOnlyList<Appointment>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default
    );

    Task<IReadOnlyList<Appointment>> GetByPatientIdAsync(
        Guid patientId,
        CancellationToken ct = default
    );

    Task<Appointment> CreateAsync(Appointment appointment, CancellationToken ct = default);

    /// <summary>
    /// Checks whether any patient under the specified user account has active future appointments.
    /// </summary>
    Task<bool> HasActiveAppointmentsForUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Checks whether a scheduling conflict exists for the specified doctor, date, and time range.
    /// </summary>
    Task<bool> HasConflictAsync(
        Guid doctorId,
        DateTime scheduledDate,
        TimeRange timeRange,
        CancellationToken ct = default
    );

    Task<IReadOnlyList<Appointment>> GetFutureScheduledByDoctorIdAsync(
        Guid doctorId,
        DateTime referenceDate,
        CancellationToken ct = default
    );

    /// <summary>
    /// Retrieves all appointments in RequiresReassignment status whose scheduled time has passed.
    /// </summary>
    Task<IReadOnlyList<Appointment>> GetExpiredDisplacedAppointmentsAsync(
        DateTime referenceTime,
        CancellationToken ct = default
    );
}
