using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="Schedule"/> persistence operations.
/// </summary>
public interface IScheduleRepository
{
    Task<List<Schedule>> GetByDoctorIdAsync(Guid doctorId, CancellationToken cancellationToken = default);

    Task<Schedule?> GetByDoctorAndDayAsync(Guid doctorId, DayOfWeek day, CancellationToken cancellationToken = default);

    Task<Schedule> CreateAsync(Schedule schedule, CancellationToken cancellationToken = default);

    Task UpdateAsync(Schedule schedule, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
