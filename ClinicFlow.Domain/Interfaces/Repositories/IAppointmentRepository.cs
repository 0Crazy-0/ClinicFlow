using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="Appointment"/> persistence operations.
/// </summary>
public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IList<Appointment>> GetByDoctorIdAsync(Guid doctorId, DateTime date, CancellationToken cancellationToken = default);

    Task<IList<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    Task<IList<Appointment>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);

    Task<Appointment> CreateAsync(Appointment appointment, CancellationToken cancellationToken = default);

    Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a scheduling conflict exists for the specified doctor, date, and time range.
    /// </summary>
    Task<bool> HasConflictAsync(Guid doctorId, DateTime scheduledDate, TimeRange timeRange, CancellationToken cancellationToken = default);

    Task<IList<Appointment>> GetUpcomingByPatientAsync(Guid patientId, CancellationToken cancellationToken = default);

    Task<IList<Appointment>> GetByStatusAsync(AppointmentStatus status, CancellationToken cancellationToken = default);
}
