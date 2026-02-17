using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Scheduling;

public class InvalidTimeRangeException(string message) : DomainException(message)
{
}
