using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Scheduling;

/// <summary>
/// Thrown when a doctor does not have an active schedule covering the requested day and time.
/// </summary>
public class DoctorNotAvailableException(string errorCode, Guid doctorId, DayOfWeek dayOfWeek)
    : DomainException(errorCode)
{
    public Guid DoctorId { get; } = doctorId;
    public DayOfWeek DayOfWeek { get; } = dayOfWeek;
}
