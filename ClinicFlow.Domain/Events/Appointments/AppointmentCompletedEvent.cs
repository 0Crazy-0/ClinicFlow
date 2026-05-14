using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Events.Appointments;

public sealed record AppointmentCompletedEvent(Appointment Appointment, DateTime CompletedAt)
    : IDomainEvent;
