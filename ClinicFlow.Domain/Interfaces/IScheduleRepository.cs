using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Interfaces;

public interface IScheduleRepository
{
    Task<List<Schedule>> GetByDoctorIdAsync(Guid doctorId);
    Task<Schedule?> GetByDoctorAndDayAsync(Guid doctorId, DayOfWeek day);
    Task<Schedule> CreateAsync(Schedule schedule);
    Task UpdateAsync(Schedule schedule);
    Task DeleteAsync(Guid id);
}
