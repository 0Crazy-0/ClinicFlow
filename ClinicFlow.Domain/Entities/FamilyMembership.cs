using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents the temporal membership relationship between a user account and a patient profile.
/// </summary>
public class FamilyMembership : BaseEntity
{
    public const int MinimumAgeToLeave = DomainRules.AdultAge;
    public const int MinimumAdultAge = DomainRules.AdultAge;
    public Guid PatientId { get; private set; }

    public Guid UserId { get; private set; }

    public PatientRelationship Role { get; private set; }

    /// <summary>
    /// Defines whether the member holds legal authority over the linked patient.
    /// </summary>
    /// <remarks>
    /// The meaning and coherence rules of this value are described in
    /// <see cref="LegalAuthorityType"/>.
    /// </remarks>
    public LegalAuthorityType LegalAuthority { get; private set; }

    public FamilyMembershipStatus Status { get; private set; }

    public FamilyMembershipAccessLevel AccessLevel { get; private set; }

    private readonly List<AppointmentCategory> _allowedAppointmentCategories = [];

    /// <summary>
    /// Lists the categories the family member is allowed to schedule appointments
    /// for when <see cref="AccessLevel"/> is
    /// <see cref="FamilyMembershipAccessLevel.Restricted"/>. An empty collection
    /// means the member cannot schedule any appointment at all; this is
    /// intentional, not a permissive fallback.
    /// </summary>
    /// <remarks>
    /// Protection status never filters scheduling. A category granted here opens
    /// both routine and protected <see cref="AppointmentTypeDefinition"/> entries
    /// of that category;
    /// <see cref="AppointmentTypeDefinition.ProtectedCareCategory"/> governs
    /// record visibility only, as enforced by
    /// <see cref="Services.Policies.ProtectedCategoryPolicy"/>.
    /// </remarks>
    public IReadOnlyCollection<AppointmentCategory> AllowedAppointmentCategories =>
        _allowedAppointmentCategories.AsReadOnly();

    public DateTime StartedAt { get; private set; }

    /// <remarks>
    /// Null while the membership is Active. Populated when the membership transitions to Revoked, Left, or Closed.
    /// </remarks>
    public DateTime? EndedAt { get; private set; }

    // EF Core parameterless constructor
    private FamilyMembership() { }

    private FamilyMembership(
        Guid patientId,
        Guid userId,
        PatientRelationship role,
        LegalAuthorityType legalAuthority,
        FamilyMembershipAccessLevel accessLevel,
        DateTime startedAt
    )
        : this()
    {
        PatientId = patientId;
        UserId = userId;
        Role = role;
        LegalAuthority = legalAuthority;
        AccessLevel = accessLevel;
        Status = FamilyMembershipStatus.Active;
        StartedAt = startedAt;
    }

    /// <summary>
    /// Creates a self membership link for the primary account owner on their own patient profile.
    /// </summary>
    internal static FamilyMembership CreateSelf(Guid patientId, Guid userId, DateTime referenceTime)
    {
        Guard.NotEmpty(patientId);
        Guard.NotEmpty(userId);
        Guard.NotDefault(referenceTime);

        return new FamilyMembership(
            patientId,
            userId,
            PatientRelationship.Self,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            referenceTime
        );
    }

    /// <summary>
    /// Creates a family member membership link for a dependent patient under an account owner.
    /// </summary>
    /// <remarks>
    /// A patient under <see cref="MinimumAdultAge"/> can only be linked with
    /// <see cref="FamilyMembershipAccessLevel.Full"/> access, since a minor's
    /// profile must remain fully manageable by their legal guardian.
    /// Role, authority and age coherence is validated as described in
    /// <see cref="LegalAuthorityType"/>.
    /// </remarks>
    internal static FamilyMembership CreateFamilyMember(
        Guid patientId,
        Guid ownerUserId,
        PatientRelationship role,
        LegalAuthorityType legalAuthority,
        FamilyMembershipAccessLevel accessLevel,
        int patientAge,
        DateTime referenceTime
    )
    {
        Guard.NotEmpty(patientId);
        Guard.NotEmpty(ownerUserId);
        Guard.NotDefault(referenceTime);
        Guard.IsDefined(role);

        if (role is PatientRelationship.Self)
            throw new DomainValidationException(DomainErrors.FamilyMembership.CannotBeSelf);

        Guard.IsDefined(legalAuthority);

        if (
            legalAuthority is LegalAuthorityType.Parent && role is not PatientRelationship.Parent
            || legalAuthority is LegalAuthorityType.Guardian
                && role is not (PatientRelationship.Other or PatientRelationship.Sibling)
        )
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.InvalidLegalAuthorityForRole
            );

        if (legalAuthority is not LegalAuthorityType.None && patientAge >= MinimumAdultAge)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.LegalAuthorityRequiresMinor
            );

        if (legalAuthority is LegalAuthorityType.None && patientAge < MinimumAdultAge)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.MinorRequiresLegalAuthority
            );

        Guard.IsDefined(accessLevel);

        if (accessLevel is FamilyMembershipAccessLevel.Unspecified)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (patientAge < MinimumAdultAge && accessLevel is not FamilyMembershipAccessLevel.Full)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.MinorMustHaveFullAccess
            );

        return new FamilyMembership(
            patientId,
            ownerUserId,
            role,
            legalAuthority,
            accessLevel,
            referenceTime
        );
    }

    /// <remarks>
    /// The access level of a patient under <see cref="MinimumAdultAge"/> is
    /// immutable: their membership must remain at
    /// <see cref="FamilyMembershipAccessLevel.Full"/> while the patient is a
    /// minor, so any change attempt is rejected regardless of the target level.
    /// </remarks>
    public void ChangeAccessLevel(
        FamilyMembershipAccessLevel newAccessLevel,
        int patientAge,
        bool requesterIsAuthorized
    )
    {
        Guard.IsDefined(newAccessLevel);

        if (newAccessLevel is FamilyMembershipAccessLevel.Unspecified)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (Role is PatientRelationship.Self)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.CannotChangeAccessLevelOfSelf
            );

        if (patientAge < MinimumAdultAge)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.CannotChangeAccessLevelWhileMinor
            );

        if (!requesterIsAuthorized)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.UnauthorizedAccessLevelChange
            );

        if (AccessLevel == newAccessLevel)
            throw new DomainValidationException(DomainErrors.FamilyMembership.AccessLevelUnchanged);

        if (
            newAccessLevel is not FamilyMembershipAccessLevel.Restricted
            && _allowedAppointmentCategories.Count > 0
        )
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.CannotChangeAccessLevelWithCategoriesConfigured
            );

        AccessLevel = newAccessLevel;
    }

    public void AddAllowedAppointmentCategory(
        AppointmentCategory category,
        bool requesterIsAuthorized
    )
    {
        if (!requesterIsAuthorized)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.UnauthorizedCategoryListChange
            );

        if (AccessLevel is not FamilyMembershipAccessLevel.Restricted)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.OnlyRestrictedCanHaveCategoryList
            );

        Guard.IsDefined(category);

        if (_allowedAppointmentCategories.Contains(category))
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.CategoryAlreadyAllowed
            );

        _allowedAppointmentCategories.Add(category);
    }

    public void RemoveAllowedAppointmentCategory(
        AppointmentCategory category,
        bool requesterIsAuthorized
    )
    {
        if (!requesterIsAuthorized)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.UnauthorizedCategoryListChange
            );

        if (AccessLevel is not FamilyMembershipAccessLevel.Restricted)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.OnlyRestrictedCanHaveCategoryList
            );

        if (!_allowedAppointmentCategories.Remove(category))
            throw new DomainValidationException(DomainErrors.FamilyMembership.CategoryNotFound);
    }

    /// <summary>
    /// Validates that the membership access level allows reading the linked patient's medical records.
    /// Only Full or ViewOnly access levels are authorized, so a family member can consult records
    /// of related patients; otherwise throws a validation exception.
    /// </summary>
    public void EnsureMedicalRecordsAccess()
    {
        if (
            AccessLevel
            is not (FamilyMembershipAccessLevel.Full or FamilyMembershipAccessLevel.ViewOnly)
        )
            throw new DomainValidationException(DomainErrors.MedicalRecord.UnauthorizedAccess);
    }

    /// <summary>
    /// Validates that the membership may act as a guardian (scheduling, rescheduling and
    /// guardian notes). Requires current Parent or Guardian authority with Full access.
    /// The patient themselves never act as guardian.
    /// </summary>
    public void EnsureGuardianAccess()
    {
        if (
            Role is PatientRelationship.Self
            || LegalAuthority is not (LegalAuthorityType.Parent or LegalAuthorityType.Guardian)
            || AccessLevel is not FamilyMembershipAccessLevel.Full
        )
            throw new PatientAccessUnauthorizedException(DomainErrors.Patient.UnauthorizedAccess);
    }

    /// <summary>
    /// Validates that the membership may edit an existing guardian note. Requires guardian
    /// access plus the original author.
    /// </summary>
    public void EnsureGuardianNotesEditAccess(Guid currentAuthorUserId)
    {
        Guard.NotEmpty(currentAuthorUserId);

        EnsureGuardianAccess();

        if (UserId != currentAuthorUserId)
            throw new PatientAccessUnauthorizedException(DomainErrors.Patient.UnauthorizedAccess);
    }

    public void Revoke(
        bool patientHasOwnSelfMembership,
        bool hasUpcomingAppointmentRequiringGuardianForMinor,
        DateTime referenceTime
    )
    {
        if (Role is PatientRelationship.Self)
            throw new DomainValidationException(DomainErrors.FamilyMembership.CannotRemoveSelf);

        if (Status is not FamilyMembershipStatus.Active)
            throw new DomainValidationException(DomainErrors.FamilyMembership.AlreadyTerminated);

        if (!patientHasOwnSelfMembership)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.CannotRemoveWithoutOwnSelf
            );

        if (hasUpcomingAppointmentRequiringGuardianForMinor)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.CannotRemoveWithUpcomingAppointments
            );

        if (referenceTime <= StartedAt)
            throw new DomainValidationException(
                DomainErrors.Validation.EndTimeMustBeAfterStartTime
            );

        Status = FamilyMembershipStatus.Revoked;
        EndedAt = referenceTime;
    }

    public void Leave(int memberAge, DateTime referenceTime)
    {
        if (Role is PatientRelationship.Self)
            throw new DomainValidationException(DomainErrors.FamilyMembership.CannotLeaveSelf);

        if (Status is not FamilyMembershipStatus.Active)
            throw new DomainValidationException(DomainErrors.FamilyMembership.AlreadyTerminated);

        if (memberAge < MinimumAgeToLeave)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.MemberMustBeAdultToLeave
            );

        if (referenceTime <= StartedAt)
            throw new DomainValidationException(
                DomainErrors.Validation.EndTimeMustBeAfterStartTime
            );

        Status = FamilyMembershipStatus.Left;
        EndedAt = referenceTime;
    }

    public void CloseSelfMembership(DateTime referenceTime)
    {
        if (Role is not PatientRelationship.Self)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.CanOnlyCloseSelfMembership
            );

        if (Status is not FamilyMembershipStatus.Active)
            throw new DomainValidationException(DomainErrors.FamilyMembership.AlreadyTerminated);

        if (referenceTime <= StartedAt)
            throw new DomainValidationException(
                DomainErrors.Validation.EndTimeMustBeAfterStartTime
            );

        Status = FamilyMembershipStatus.Closed;
        EndedAt = referenceTime;
    }
}
