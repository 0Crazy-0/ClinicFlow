using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Events;

/// <summary>
/// Raised when an appointment transitions to the completed status.
/// </summary>
public sealed record AppointmentCompletedEvent(Appointment Appointment, DateTime CompletedAt)
    : IDomainEvent;
