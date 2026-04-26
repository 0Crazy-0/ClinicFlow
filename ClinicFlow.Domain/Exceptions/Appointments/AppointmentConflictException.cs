using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Appointments;

public class AppointmentConflictException(string errorCode, Guid doctorId, DateTime date)
    : DomainException(errorCode)
{
    public Guid DoctorId { get; } = doctorId;
    public DateTime Date { get; } = date;
}
