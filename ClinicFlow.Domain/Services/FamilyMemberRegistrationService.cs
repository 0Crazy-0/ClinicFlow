using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Registration;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Registers a family member patient profile, either by reusing an existing
/// clinical record or creating a new one, and links it to the owner's account
/// via a new FamilyMembership.
/// </summary>
/// <remarks>
/// Encapsulates three independent business rules: soft-deleted patients cannot
/// be reused directly and must go through the administrative claim process, an
/// owner cannot link the same patient twice while a FamilyMembership between
/// them is still active, and an owner cannot exceed the maximum number of
/// active family members.
/// </remarks>
public static class FamilyMemberRegistrationService
{
    public const int MaxActiveFamilyMembers = 15;

    public static (Patient Patient, FamilyMembership Membership) Register(
        FamilyMemberRegistrationArgs args
    )
    {
        if (args.ExistingPatient is not null && args.ExistingPatient.IsDeleted)
            throw new DomainValidationException(
                DomainErrors.Patient.ProfileRequiresAdministrativeClaim
            );

        if (args.HasExistingMembershipWithOwner)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.PatientAlreadyHasActiveMembership
            );

        if (args.ActiveFamilyMemberCount >= MaxActiveFamilyMembers)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.MaxActiveFamilyMembersExceeded
            );

        var patient =
            args.ExistingPatient
            ?? Patient.CreateProfile(args.FullName, args.DateOfBirth, args.ReferenceTime);

        var membership = FamilyMembership.CreateFamilyMember(
            patient.Id,
            args.OwnerUserId,
            args.Role,
            args.ReferenceTime
        );

        return (patient, membership);
    }
}
