using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="Schedule"/> persistence operations.
/// </summary>
public interface IScheduleRepository
{
    /// <summary>
    /// Retrieves all schedule slots for a given doctor.
    /// </summary>
    Task<List<Schedule>> GetByDoctorIdAsync(Guid doctorId);

    /// <summary>
    /// Retrieves a doctor's schedule slot for a specific day of the week.
    /// </summary>
    Task<Schedule?> GetByDoctorAndDayAsync(Guid doctorId, DayOfWeek day);

    /// <summary>
    /// Persists a new schedule slot.
    /// </summary>
    Task<Schedule> CreateAsync(Schedule schedule);

    /// <summary>
    /// Updates an existing schedule slot.
    /// </summary>
    Task UpdateAsync(Schedule schedule);

    /// <summary>
    /// Deletes a schedule slot by its identifier.
    /// </summary>
    Task DeleteAsync(Guid id);
}
