using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

public class AppointmentConfirmationNotAllowedException(string reason) : DomainException($"Cannot confirm appointment: {reason}")
{
}
