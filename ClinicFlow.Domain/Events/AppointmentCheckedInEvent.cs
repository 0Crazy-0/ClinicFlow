using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Events;

/// <summary>
/// Raised when an appointment transitions to the checked in status.
/// </summary>
public sealed record AppointmentCheckedInEvent(Appointment Appointment, DateTime CheckedInAt)
    : IDomainEvent;
