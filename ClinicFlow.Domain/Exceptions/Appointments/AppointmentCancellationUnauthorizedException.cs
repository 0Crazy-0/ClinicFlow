using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

public class AppointmentCancellationUnauthorizedException(string message) : DomainException(message)
{
}
