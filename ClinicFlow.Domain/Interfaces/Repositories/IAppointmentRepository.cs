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
    /// </remarks>
    Task<(IReadOnlyList<Appointment> Items, int TotalCount)> GetByPatientIdInCategoriesAsync(
        Guid patientId,
        IReadOnlyCollection<AppointmentCategory> allowedCategories,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <remarks>
    /// Minor only visibility for family member listings. A row is visible when its type
    /// holds no protected category, when its category is not excluded for the patient's
    /// age, when a category or specific appointment grant covers it, or when the
    /// requester scheduled it. The scheduler exception always applies here: every active
    /// membership over a minor carries Parent or Guardian authority with Full access by
    /// domain invariant, so identity equality is the only scheduler condition left. No
    /// appointment category filter applies either: minor memberships are always Full by
    /// the same invariant.
    /// </remarks>
    Task<(IReadOnlyList<Appointment> Items, int TotalCount)> GetMinorVisibleAppointmentsAsync(
        Guid patientId,
        IReadOnlyCollection<ProtectedCategory> excludedCategories,
        IReadOnlyCollection<ProtectedCategory> authorizedCategories,
        IReadOnlyCollection<Guid> authorizedAppointmentIds,
        Guid requesterUserId,
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
