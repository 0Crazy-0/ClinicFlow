using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Events;

public sealed record AppointmentCompletedEvent(Appointment Appointment, DateTime CompletedAt)
    : IDomainEvent;
