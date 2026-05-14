using ClinicFlow.Domain.Common;

namespace ClinicFlow.Domain.Events;

public sealed record ScheduleDeactivatedEvent(Guid ScheduleId, Guid DoctorId, DayOfWeek DayOfWeek)
    : IDomainEvent;
