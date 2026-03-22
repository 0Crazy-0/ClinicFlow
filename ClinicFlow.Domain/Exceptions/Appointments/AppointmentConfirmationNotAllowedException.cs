using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

/// <summary>
/// Thrown when an appointment cannot be confirmed due to its current status.
/// </summary>
public class AppointmentConfirmationNotAllowedException(string errorCode)
    : DomainException(errorCode) { }
