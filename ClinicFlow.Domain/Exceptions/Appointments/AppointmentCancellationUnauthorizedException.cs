using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

/// <summary>
/// Thrown when a user lacks permission to cancel a specific appointment.
/// </summary>
public class AppointmentCancellationUnauthorizedException(string errorCode)
    : DomainException(errorCode) { }
