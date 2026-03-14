using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

/// <summary>
/// Thrown when a new appointment overlaps with an existing one for the same doctor.
/// </summary>
public class AppointmentConflictException(string errorCode, Guid doctorId, DateTime date) : DomainException(errorCode)
{
    public Guid DoctorId { get; } = doctorId;
    public DateTime Date { get; } = date;
}
