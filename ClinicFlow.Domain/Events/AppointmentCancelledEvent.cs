using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Events;

/// <summary>
/// Raised when an appointment is cancelled, carrying cancellation details.
/// </summary>
public record AppointmentCancelledEvent(Appointment Appointment, Guid CancelledByUserId, string? Reason) : IDomainEvent;