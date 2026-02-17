using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

public class InvalidAppointmentException(string message) : DomainException(message)
{
}
