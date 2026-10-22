using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.Services.Contexts.Registration;

namespace ClinicFlow.Domain.Services;

/// <summary>
/// Registers a family member patient profile, either by reusing an existing
/// clinical record or creating a new one, and links it to the owner's account
/// via a new FamilyMembership.
/// </summary>
public static class FamilyMemberRegistrationService
{
    public const int MaxActiveFamilyMembers = 15;
    public const int MinimumOwnerAge = DomainRules.AdultAge;

    public static (Patient Patient, FamilyMembership Membership) Register(
        FamilyMemberRegistrationArgs args,
        FamilyMemberRegistrationContext context
    )
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.FullName);
        ArgumentNullException.ThrowIfNull(context);

        if (context.OwnerAgeInYears < MinimumOwnerAge)
            throw new DomainValidationException(DomainErrors.FamilyMembership.OwnerMustBeAdult);

        if (context.HasExistingMembershipWithOwner)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.PatientAlreadyHasActiveMembership
            );

        if (context.ActiveFamilyMemberCount >= MaxActiveFamilyMembers)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.MaxActiveFamilyMembersExceeded
            );

        if (
            context.ExistingPatient is not null
            && (
                context.ExistingPatient.SexAtBirth != args.SexAtBirth
                || context.ExistingPatient.HomeAddress != args.HomeAddress
            )
        )
            throw new DomainValidationException(DomainErrors.Patient.DataMismatch);

        var patient =
            context.ExistingPatient
            ?? Patient.CreateProfile(
                args.FullName,
                args.DateOfBirth,
                args.SexAtBirth,
                args.HomeAddress,
                context.ReferenceTime
            );

        var membership = FamilyMembership.CreateFamilyMember(
            patient.Id,
            context.OwnerUserId,
            args.Role,
            args.LegalAuthority,
            args.AccessLevel,
            patient.GetAge(DateOnly.FromDateTime(context.ReferenceTime)),
            context.ReferenceTime
        );

        return (patient, membership);
    }
}
