using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Events;

/// <summary>
/// Domain event published when an appointment is marked as a no-show.
/// </summary>
public record AppointmentMarkedAsNoShowEvent(Appointment Appointment) : IDomainEvent;
