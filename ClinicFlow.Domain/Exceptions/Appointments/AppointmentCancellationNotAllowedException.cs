using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

/// <summary>
/// Thrown when an appointment cannot be cancelled due to its current status.
/// </summary>
public class AppointmentCancellationNotAllowedException(AppointmentStatus currentStatus) : DomainException($"Cannot cancel appointment. Current status: {currentStatus}")
{
    /// <summary>
    /// The appointment status that prevented cancellation.
    /// </summary>
    public AppointmentStatus CurrentStatus { get; } = currentStatus;
}
