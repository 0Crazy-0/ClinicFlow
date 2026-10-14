using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Events.Appointments;

public sealed record AppointmentStartedEvent(Appointment SubjectAppointment, DateTime StartedAt)
    : IDomainEvent;
