using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Events;

/// <summary>
/// Raised when a new appointment is successfully scheduled.
/// </summary>
public record AppointmentScheduledEvent(Appointment Appointment) : IDomainEvent;
