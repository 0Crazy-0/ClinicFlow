using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="Schedule"/> persistence operations.
/// </summary>
public interface IScheduleRepository
{
    Task CreateAsync(Schedule schedule, CancellationToken cancellationToken = default);

    Task CreateRangeAsync(
        IReadOnlyList<Schedule> schedules,
        CancellationToken cancellationToken = default
    );

    /// <remarks>
    /// Returns schedules regardless of their <see cref="Schedule.IsActive"/> state.
    /// </remarks>
    Task<Schedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Schedule?> GetActiveByDoctorAndDayAsync(
        Guid doctorId,
        DayOfWeek day,
        CancellationToken cancellationToken = default
    );

    /// <remarks>
    /// Returns schedules regardless of their <see cref="Schedule.IsActive"/> state.
    /// </remarks>
    Task<IReadOnlyList<Schedule>> GetByDoctorIdAsync(
        Guid doctorId,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Schedule>> GetActiveByDoctorIdAsync(
        Guid doctorId,
        CancellationToken cancellationToken = default
    );
}
