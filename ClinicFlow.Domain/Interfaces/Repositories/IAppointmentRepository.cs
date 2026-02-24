using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="Appointment"/> persistence operations.
/// </summary>
public interface IAppointmentRepository
{
    /// <summary>
    /// Retrieves an appointment by its unique identifier.
    /// </summary>
    Task<Appointment?> GetByIdAsync(Guid id);

    /// <summary>
    /// Retrieves all appointments for a given doctor on a specific date.
    /// </summary>
    Task<IList<Appointment>> GetByDoctorIdAsync(Guid doctorId, DateTime date);

    /// <summary>
    /// Retrieves all appointments for a given patient.
    /// </summary>
    Task<IList<Appointment>> GetByPatientIdAsync(Guid patientId);

    /// <summary>
    /// Persists a new appointment.
    /// </summary>
    Task<Appointment> CreateAsync(Appointment appointment);

    /// <summary>
    /// Updates an existing appointment.
    /// </summary>
    Task UpdateAsync(Appointment appointment);

    /// <summary>
    /// Deletes an appointment by its identifier.
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Checks whether a scheduling conflict exists for the specified doctor, date, and time range.
    /// </summary>
    Task<bool> HasConflictAsync(Guid doctorId, DateTime scheduledDate, TimeRange timeRange);

    /// <summary>
    /// Retrieves upcoming (future) appointments for a patient.
    /// </summary>
    Task<IList<Appointment>> GetUpcomingByPatientAsync(Guid patientId);

    /// <summary>
    /// Retrieves all appointments that match the specified status.
    /// </summary>
    Task<IList<Appointment>> GetByStatusAsync(AppointmentStatus status);
}
