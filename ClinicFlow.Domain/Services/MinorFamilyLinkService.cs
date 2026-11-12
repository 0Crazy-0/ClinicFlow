using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.FamilyMemberships;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.Services.Contexts.Registration;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Links a minor patient to their legal guardian's account as a family member with full access.
/// </summary>
/// <remarks>
/// The coherence between role, legal authority and the minor's age is delegated to
/// <see cref="FamilyMembership.CreateMinorFamilyMember"/>, which owns those rules
/// and always produces a Full access membership.
/// </remarks>
public static class MinorFamilyLinkService
{
    /// <summary>
    /// Maximum number of memberships in Active status a guardian account may hold.
    /// </summary>
    public const int MaxActiveFamilyMembers = 15;

    /// <remarks>
    /// The caller must ensure the minor patient has no active membership with the guardian
    /// at the time of the link, whether the minor is linked during registration or as an
    /// already registered patient. The service does not validate that precondition.
    /// </remarks>
    public static FamilyMembership Link(
        Patient minorPatient,
        MinorFamilyLinkArgs args,
        MinorFamilyLinkContext context
    )
    {
        ArgumentNullException.ThrowIfNull(minorPatient);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.GuardianPatient);
        ArgumentNullException.ThrowIfNull(context.GuardianSelfMembership);

        var referenceDate = DateOnly.FromDateTime(context.ReferenceTime);
        var minorAge = minorPatient.GetAge(referenceDate);

        if (minorAge >= DomainRules.AdultAge)
            throw new DomainValidationException(DomainErrors.PatientRegistration.MinorRequired);

        if (context.GuardianSelfMembership.Status is not FamilyMembershipStatus.Active)
            throw new DomainValidationException(DomainErrors.Patient.PrimaryPatientRequired);

        var guardianAge = context.GuardianPatient.GetAge(referenceDate);

        if (guardianAge < DomainRules.AdultAge)
            throw new DomainValidationException(DomainErrors.FamilyMembership.OwnerMustBeAdult);

        if (context.ActiveFamilyMemberCount >= MaxActiveFamilyMembers)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.MaxActiveFamilyMembersExceeded
            );

        var membership = FamilyMembership.CreateMinorFamilyMember(
            minorPatient.Id,
            context.GuardianSelfMembership.UserId,
            args.Relationship,
            args.LegalAuthority,
            minorAge,
            context.ReferenceTime
        );

        membership.AddDomainEvent(
            new MinorLinkedToGuardianEvent(
                minorPatient.Id,
                context.GuardianSelfMembership.UserId,
                args.Relationship,
                context.ReferenceTime
            )
        );

        return membership;
    }
}
