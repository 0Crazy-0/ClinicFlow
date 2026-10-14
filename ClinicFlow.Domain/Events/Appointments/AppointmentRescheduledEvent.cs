using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Events.Appointments;

public sealed record AppointmentRescheduledEvent(
    Appointment SubjectAppointment,
    DateOnly PreviousDate,
    TimeRange PreviousTimeRange
) : IDomainEvent;
