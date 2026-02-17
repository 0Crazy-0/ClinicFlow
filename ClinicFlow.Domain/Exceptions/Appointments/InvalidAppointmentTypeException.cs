using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

public class InvalidAppointmentTypeException(string message) : DomainException(message)
{
}
