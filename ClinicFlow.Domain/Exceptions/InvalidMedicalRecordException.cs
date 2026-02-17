namespace ClinicFlow.Domain.Exceptions;

public class InvalidMedicalRecordException(string message) : DomainException(message)
{
}