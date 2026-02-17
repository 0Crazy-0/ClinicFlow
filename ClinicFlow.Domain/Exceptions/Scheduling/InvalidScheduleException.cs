using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Scheduling;

public class InvalidScheduleException(string message) : DomainException(message)
{
}
