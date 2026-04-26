using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Events;

public sealed record AppointmentRescheduledEvent(
    Appointment Appointment,
    DateTime PreviousDate,
    TimeRange PreviousTimeRange
) : IDomainEvent;
