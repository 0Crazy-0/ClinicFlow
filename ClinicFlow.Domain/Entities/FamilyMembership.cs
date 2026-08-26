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
        DateTime startedAt
    )
        : this()
    {
        PatientId = patientId;
        UserId = userId;
        Role = role;
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

        return new FamilyMembership(patientId, userId, PatientRelationship.Self, referenceTime);
    }

    /// <summary>
    /// Creates a family member membership link for a dependent patient under an account owner.
    /// </summary>
    internal static FamilyMembership CreateFamilyMember(
        Guid patientId,
        Guid ownerUserId,
        PatientRelationship role,
        DateTime referenceTime
    )
    {
        if (patientId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (ownerUserId == Guid.Empty)
            throw new DomainValidationException(DomainErrors.Validation.ValueRequired);

        if (!Enum.IsDefined(role))
            throw new DomainValidationException(DomainErrors.Validation.InvalidEnumValue);

        if (role is PatientRelationship.Self)
            throw new DomainValidationException(DomainErrors.FamilyMembership.CannotBeSelf);

        return new FamilyMembership(patientId, ownerUserId, role, referenceTime);
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
