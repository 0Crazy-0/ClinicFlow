using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

/// <summary>
/// Thrown when a user lacks permission to mark a specific appointment as a no-show.
/// </summary>
public class AppointmentNoShowUnauthorizedException(string message) : DomainException(message)
{
}
