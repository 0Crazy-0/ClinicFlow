using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Events;

public sealed record AppointmentLateCancelledEvent(
    Appointment Appointment,
    Guid CancelledByUserId,
    string? Reason
) : IDomainEvent;
