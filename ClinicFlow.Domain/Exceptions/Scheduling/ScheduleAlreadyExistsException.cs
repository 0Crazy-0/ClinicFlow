using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Scheduling;

/// <summary>
/// Thrown when attempting to create a schedule for a day that already has an active schedule for the same doctor.
/// </summary>
public class ScheduleAlreadyExistsException(string errorCode, Guid doctorId, DayOfWeek dayOfWeek)
    : DomainException(errorCode)
{
    public Guid DoctorId { get; } = doctorId;
    public DayOfWeek DayOfWeek { get; } = dayOfWeek;
}
