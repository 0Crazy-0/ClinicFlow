using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Events.Appointments;

public sealed record AppointmentCheckedInEvent(Appointment Appointment, DateOnly CheckedInAt)
    : IDomainEvent;
