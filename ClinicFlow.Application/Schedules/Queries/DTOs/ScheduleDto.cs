namespace ClinicFlow.Application.Schedules.Queries.DTOs;

public sealed record ScheduleDto(
    Guid Id,
    Guid DoctorId,
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    bool IsActive
);
