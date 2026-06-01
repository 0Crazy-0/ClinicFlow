using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Domain service responsible for verifying patient relationship and access permissions.
/// </summary>
public static class PatientAccessService
{
    public static void EnsureCanActOnBehalfOf(Patient initiator, Patient target)
    {
        if (initiator is null || target is null)
            throw new DomainValidationException(DomainErrors.General.RequiredFieldNull);

        if (initiator.UserId != target.UserId)
            throw new PatientAccessUnauthorizedException(DomainErrors.Patient.UnauthorizedAccess);

        if (
            initiator.RelationshipToUser is not PatientRelationship.Self
            && initiator.Id != target.Id
        )
            throw new PatientAccessUnauthorizedException(DomainErrors.Patient.UnauthorizedAccess);
    }
}
