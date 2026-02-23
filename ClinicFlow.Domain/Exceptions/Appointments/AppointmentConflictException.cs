using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

/// <summary>
/// Thrown when a new appointment overlaps with an existing one for the same doctor.
/// </summary>
public class AppointmentConflictException(Guid doctorId, DateTime date) : DomainException($"Doctor {doctorId} already has an appointment scheduled at {date:yyyy-MM-dd HH:mm}")
{
}
