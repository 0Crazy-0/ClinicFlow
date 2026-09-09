using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Entities;

/// <summary>
/// Represents the temporal membership relationship between a user account and a patient profile.
/// </summary>
public class FamilyMembership : BaseEntity
{
    public const int MinimumAgeToLeave = 18;
    public Guid PatientId { get; private set; }

    public Guid UserId { get; private set; }

    public PatientRelationship Role { get; private set; }

    public FamilyMembershipStatus Status { get; private set; }

    public FamilyMembershipAccessLevel AccessLevel { get; private set; }

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
        FamilyMembershipAccessLevel accessLevel,
        DateTime startedAt
    )
        : this()
    {
        PatientId = patientId;
        UserId = userId;
        Role = role;
        AccessLevel = accessLevel;
        Status = FamilyMembershipStatus.Active;
        StartedAt = startedAt;
    }

    /// <summary>
    /// Creates a self membership link for the primary account owner on their own patient profile.
    /// </summary>
    internal static FamilyMembership CreateSelf(Guid patientId, Guid userId, DateTime referenceTime)
    {
        if (patientId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (userId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (referenceTime == default)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        return new FamilyMembership(
            patientId,
            userId,
            PatientRelationship.Self,
            FamilyMembershipAccessLevel.Full,
            referenceTime
        );
    }

    /// <summary>
    /// Creates a family member membership link for a dependent patient under an account owner.
    /// </summary>
    internal static FamilyMembership CreateFamilyMember(
        Guid patientId,
        Guid ownerUserId,
        PatientRelationship role,
        FamilyMembershipAccessLevel accessLevel,
        DateTime referenceTime
    )
    {
        if (patientId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (ownerUserId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (referenceTime == default)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (!Enum.IsDefined(role))
            throw new DomainValidationException(DomainErrors.Validation.InvalidEnumValue);

        if (role is PatientRelationship.Self)
            throw new DomainValidationException(DomainErrors.FamilyMembership.CannotBeSelf);

        if (!Enum.IsDefined(accessLevel))
            throw new DomainValidationException(DomainErrors.Validation.InvalidEnumValue);

        if (accessLevel is FamilyMembershipAccessLevel.Unspecified)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        return new FamilyMembership(patientId, ownerUserId, role, accessLevel, referenceTime);
    }

    public void ChangeAccessLevel(
        FamilyMembershipAccessLevel newAccessLevel,
        bool requesterIsAuthorized
    )
    {
        if (!Enum.IsDefined(newAccessLevel))
            throw new DomainValidationException(DomainErrors.Validation.InvalidEnumValue);

        if (newAccessLevel is FamilyMembershipAccessLevel.Unspecified)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (Role is PatientRelationship.Self)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.CannotChangeAccessLevelOfSelf
            );

        if (!requesterIsAuthorized)
            throw new DomainValidationException(
                DomainErrors.FamilyMembership.UnauthorizedAccessLevelChange
            );

        if (AccessLevel == newAccessLevel)
            throw new DomainValidationException(DomainErrors.FamilyMembership.AccessLevelUnchanged);

        AccessLevel = newAccessLevel;
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
