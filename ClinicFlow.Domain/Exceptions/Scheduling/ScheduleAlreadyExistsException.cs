using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Scheduling;

public class ScheduleAlreadyExistsException(string errorCode, Guid doctorId, DayOfWeek dayOfWeek)
    : DomainException(errorCode)
{
    public Guid DoctorId { get; } = doctorId;
    public DayOfWeek DayOfWeek { get; } = dayOfWeek;
}
