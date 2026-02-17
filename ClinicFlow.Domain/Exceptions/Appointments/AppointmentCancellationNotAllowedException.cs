using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

public class AppointmentCancellationNotAllowedException(AppointmentStatus currentStatus) : DomainException($"Cannot cancel appointment. Current status: {currentStatus}")
{
    public AppointmentStatus CurrentStatus { get; } = currentStatus;
}
