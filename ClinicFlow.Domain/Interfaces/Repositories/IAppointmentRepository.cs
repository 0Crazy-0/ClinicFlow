using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="Appointment"/> persistence operations.
/// </summary>
public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id);

    Task<IList<Appointment>> GetByDoctorIdAsync(Guid doctorId, DateTime date);

    Task<IList<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    Task<IList<Appointment>> GetByPatientIdAsync(Guid patientId);

    Task<Appointment> CreateAsync(Appointment appointment);

    Task UpdateAsync(Appointment appointment);

    Task DeleteAsync(Guid id);

    /// <summary>
    /// Checks whether a scheduling conflict exists for the specified doctor, date, and time range.
    /// </summary>
    Task<bool> HasConflictAsync(Guid doctorId, DateTime scheduledDate, TimeRange timeRange);

    Task<IList<Appointment>> GetUpcomingByPatientAsync(Guid patientId);

    Task<IList<Appointment>> GetByStatusAsync(AppointmentStatus status);
}
