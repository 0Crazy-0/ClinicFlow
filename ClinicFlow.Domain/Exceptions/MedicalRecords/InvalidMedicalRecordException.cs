using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.MedicalRecords;

public class InvalidMedicalRecordException(string message) : DomainException(message)
{
}
