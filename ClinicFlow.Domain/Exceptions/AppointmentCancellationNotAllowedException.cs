using ClinicFlow.Domain.Enums;

namespace ClinicFlow.Domain.Exceptions;

public class AppointmentCancellationNotAllowedException(AppointmentStatus currentStatus) : DomainException($"Cannot cancel appointment. Current status: {currentStatus}")
{
    public AppointmentStatus CurrentStatus { get; } = currentStatus;
}