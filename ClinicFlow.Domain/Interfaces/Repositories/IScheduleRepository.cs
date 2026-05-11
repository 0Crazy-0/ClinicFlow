using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="Schedule"/> persistence operations.
/// </summary>
public interface IScheduleRepository
{
    Task<Schedule?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Schedule?> GetByDoctorAndDayAsync(
        Guid doctorId,
        DayOfWeek day,
        CancellationToken ct = default
    );

    Task<IReadOnlyList<Schedule>> GetByDoctorIdAsync(Guid doctorId, CancellationToken ct = default);

    Task<Schedule> CreateAsync(Schedule schedule, CancellationToken ct = default);

    Task CreateRangeAsync(IReadOnlyList<Schedule> schedules, CancellationToken ct = default);
}
