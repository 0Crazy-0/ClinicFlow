using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Exceptions;

public class AppointmentCancellationNotAllowedException(AppointmentStatusEnum currentStatus) 
    : DomainException($"Cannot cancel appointment. Current status: {currentStatus}")
{
    public AppointmentStatusEnum CurrentStatus { get; } = currentStatus;
}
