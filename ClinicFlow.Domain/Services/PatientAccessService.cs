using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Exceptions.Patients;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Verifies patient access permissions based on pre-resolved membership evaluation.
/// </summary>
public static class PatientAccessService
{
    public static void VerifyAccess(bool initiatorHasAccessToTarget)
    {
        if (!initiatorHasAccessToTarget)
            throw new PatientAccessUnauthorizedException(DomainErrors.Patient.UnauthorizedAccess);
    }
}
