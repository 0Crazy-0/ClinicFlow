using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Events;

/// <summary>
/// Raised when an appointment transitions to the in-progress status.
/// </summary>
public sealed record AppointmentStartedEvent(Appointment Appointment, DateTime StartedAt)
    : IDomainEvent;
