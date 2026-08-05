using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Exceptions.Patients;

public class ActiveFamilyMembersExistException(string errorCode, Guid userId)
    : DomainException(errorCode)
{
    public Guid UserId { get; } = userId;
}
