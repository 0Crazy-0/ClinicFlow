namespace ClinicFlow.Domain.Services.Args.Schedule;

public sealed record WeeklyScheduleSlot(DayOfWeek DayOfWeek, TimeSpan StartTime, TimeSpan EndTime);
