using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
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

    /// <remarks>
    /// Both filters resolve in SQL before pagination, so the total count only reflects
    /// visible appointments. An empty allowed list matches nothing by design.
    /// An appointment type is visible when its category is allowed and its protected
    /// care category is either absent or not excluded. The guardian based exceptions
    /// of the medical record consent regime are deliberately not evaluated here: they
    /// depend on per record state held by <see cref="MedicalRecord"/> (guardian initiated
    /// treatment and guardian involvement determinations), which an appointment does not
    /// carry. An appointment type protected by age is therefore always hidden from
    /// family member listings.
    /// </remarks>
    Task<(
        IReadOnlyList<Appointment> Items,
        int TotalCount
    )> GetByPatientIdInCategoriesExcludingProtectedAsync(
        Guid patientId,
        IReadOnlyCollection<AppointmentCategory> allowedCategories,
        IReadOnlyCollection<ProtectedCategory> excludedCategories,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <remarks>
    /// An appointment is active while its <see cref="AppointmentStatus"/> is
    /// <see cref="AppointmentStatus.Scheduled"/> or
    /// <see cref="AppointmentStatus.RequiresReassignment"/>.
    /// </remarks>
    Task<bool> HasActiveAppointmentForPatientAsync(
        Guid patientId,
        Guid appointmentTypeId,
        Guid? excludeAppointmentId = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> HasUpcomingAppointmentRequiringGuardianForMinorAsync(
        Guid patientId,
        DateTime referenceTime,
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
