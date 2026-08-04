using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Patients;

public class PrimaryPatientRequiredException(string errorCode, Guid userId)
    : DomainException(errorCode)
{
    public Guid UserId { get; } = userId;
}
