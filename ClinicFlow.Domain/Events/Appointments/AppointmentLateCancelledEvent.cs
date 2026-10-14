using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Events.Appointments;

public sealed record AppointmentLateCancelledEvent(
    Appointment SubjectAppointment,
    Guid CancelledByUserId,
    string? Reason
) : IDomainEvent;
