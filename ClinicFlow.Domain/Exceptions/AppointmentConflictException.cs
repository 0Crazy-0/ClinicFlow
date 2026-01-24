namespace ClinicFlow.Domain.Exceptions;

public class AppointmentConflictException(Guid doctorId, DateTime date)
: DomainException($"Doctor {doctorId} already has an appointment scheduled at {date:yyyy-MM-dd HH:mm}")
{
}
