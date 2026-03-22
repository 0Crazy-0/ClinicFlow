using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Events;

/// <summary>
/// Raised when an appointment is rescheduled, carrying the previous date and time range.
/// </summary>
public record AppointmentRescheduledEvent(
    Appointment Appointment,
    DateTime PreviousDate,
    TimeRange PreviousTimeRange
) : IDomainEvent;
