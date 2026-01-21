namespace ClinicFlow.Domain.Exceptions
{
    public class InvalidScheduleException(string message) : DomainException(message)
    {
    }
}