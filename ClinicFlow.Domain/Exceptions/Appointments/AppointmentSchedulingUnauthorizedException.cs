using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

/// <summary>
/// Thrown when a user lacks permission to schedule a specific appointment.
/// </summary>
public class AppointmentSchedulingUnauthorizedException(string errorCode)
    : DomainException(errorCode) { }
