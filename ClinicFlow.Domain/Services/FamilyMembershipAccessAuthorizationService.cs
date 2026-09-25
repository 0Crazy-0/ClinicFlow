using ClinicFlow.Domain.Services.Contexts;

namespace ClinicFlow.Domain.Services;

public static class FamilyMembershipAccessAuthorizationService
{
    public const int MinimumAdultAge = 18;

    /// <remarks>
    /// A requester can never manage their own membership through this check.
    /// When the patient is under <see cref="MinimumAdultAge"/>, this always
    /// returns false, regardless of whether the patient already holds a
    /// <see cref="PatientRelationship.Self"/> membership: a minor's legal
    /// guardian must hold <see cref="FamilyMembershipAccessLevel.Full"/>, so no
    /// change to another family member's access can be authorized without
    /// violating that invariant.
    /// Once the patient reaches <see cref="MinimumAdultAge"/>, only the patient
    /// themself, acting through their own <see cref="PatientRelationship.Self"/>
    /// membership, may manage another family member's access.
    /// </remarks>
    public static bool CanManageFamilyMembership(
        FamilyMembershipManagementAuthorizationContext context
    )
    {
        if (context.RequesterUserId == context.TargetUserId)
            return false;

        var patientIsMinor =
            context.Patient.GetAge(DateOnly.FromDateTime(context.ReferenceTime)) < MinimumAdultAge;

        if (patientIsMinor)
            return false;

        return context.RequesterIsPatientsSelf;
    }
}
