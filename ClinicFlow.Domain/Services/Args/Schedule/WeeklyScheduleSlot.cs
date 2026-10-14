namespace ClinicFlow.Domain.Services.Args.Schedule;

public sealed record WeeklyScheduleSlot(DayOfWeek Weekday, TimeOnly StartTime, TimeOnly EndTime);
