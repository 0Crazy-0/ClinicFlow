using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Scheduling;

/// <summary>
/// Thrown when a time range is invalid (e.g., start is after end, or the range is null).
/// </summary>
public class InvalidTimeRangeException(string errorCode) : DomainException(errorCode)
{
}
