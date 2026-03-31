using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Events;

/// <summary>
/// Raised when an appointment transitions to the confirmed status.
/// </summary>
public sealed record AppointmentConfirmedEvent(Appointment Appointment) : IDomainEvent;
