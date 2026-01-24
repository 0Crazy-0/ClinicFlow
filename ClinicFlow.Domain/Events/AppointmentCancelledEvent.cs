using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;

namespace ClinicFlow.Domain.Events
{
    public record AppointmentCancelledEvent(Appointment Appointment, Guid CancelledByUserId, string? Reason) : IDomainEvent;
}
