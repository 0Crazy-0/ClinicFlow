namespace ClinicFlow.Domain.Services.Args.Schedule;

public sealed record WeeklyScheduleSlot(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);
