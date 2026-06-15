namespace ClinicFlow.Application.Schedules.Queries.DTOs;

public sealed record ScheduleDto(
    Guid Id,
    Guid DoctorId,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive
);
