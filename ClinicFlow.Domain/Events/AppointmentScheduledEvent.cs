using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Events;

public record AppointmentScheduledEvent(Appointment Appointment) : IDomainEvent;
