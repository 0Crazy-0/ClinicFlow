using ClinicFlow.Domain.Services.Contexts;

namespace ClinicFlow.Domain.Services;

public static class FamilyMembershipAccessAuthorizationService
{
    public const int MinimumAdultAge = 18;

    public static bool CanChangeAccessLevel(AccessLevelChangeAuthorizationContext context)
    {
        var patientIsMinor =
            context.Patient.GetAge(DateOnly.FromDateTime(context.ReferenceTime)) < MinimumAdultAge;

        if (patientIsMinor)
            return context.RequesterHasSelfMembership
                && context.RequesterHasActiveMembershipWithPatient;

        return context.RequesterIsPatientsSelf;
    }
}
