using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.Events.Schedules;

public sealed record ScheduleDeactivatedEvent(Guid ScheduleId, Guid DoctorId, DayOfWeek DayOfWeek)
    : IDomainEvent;
