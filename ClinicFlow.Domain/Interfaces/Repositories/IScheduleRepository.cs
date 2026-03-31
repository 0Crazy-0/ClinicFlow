using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces.Repositories;

/// <summary>
/// Repository contract for <see cref="Schedule"/> persistence operations.
/// </summary>
public interface IScheduleRepository
{
    Task<Schedule?> GetByDoctorAndDayAsync(
        Guid doctorId,
        DayOfWeek day,
        CancellationToken cancellationToken = default
    );
}
