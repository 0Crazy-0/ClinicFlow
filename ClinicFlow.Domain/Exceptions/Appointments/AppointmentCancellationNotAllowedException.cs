using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

/// <summary>
/// Thrown when an appointment cannot be cancelled due to its current status.
/// </summary>
public class AppointmentCancellationNotAllowedException(
    string errorCode,
    AppointmentStatus currentStatus
) : DomainException(errorCode)
{
    public AppointmentStatus CurrentStatus { get; } = currentStatus;
}
