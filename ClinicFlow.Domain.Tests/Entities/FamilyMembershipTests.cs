using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Entities;

public class FamilyMembershipTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void CreateSelf_ShouldCreateActiveMembership_WhenValidParameters()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        var membership = FamilyMembership.CreateSelf(patientId, userId, referenceTime);

        // Assert
        membership.PatientId.Should().Be(patientId);
        membership.UserId.Should().Be(userId);
        membership.Role.Should().Be(PatientRelationship.Self);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);
        membership.StartedAt.Should().Be(referenceTime);
        membership.EndedAt.Should().BeNull();
    }

    [Fact]
    public void CreateSelf_ShouldThrowException_WhenPatientIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            FamilyMembership.CreateSelf(
                Guid.Empty,
                Guid.CreateVersion7(),
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CreateSelf_ShouldThrowException_WhenUserIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            FamilyMembership.CreateSelf(
                Guid.CreateVersion7(),
                Guid.Empty,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CreateSelf_ShouldThrowException_WhenReferenceTimeIsDefault()
    {
        // Arrange & Act
        var act = () =>
            FamilyMembership.CreateSelf(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                default
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CreateFamilyMember_ShouldCreateActiveMembership_WhenValidParameters()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var ownerUserId = Guid.CreateVersion7();
        var role = PatientRelationship.Child;
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        var membership = FamilyMembership.CreateFamilyMember(
            patientId,
            ownerUserId,
            role,
            referenceTime
        );

        // Assert
        membership.PatientId.Should().Be(patientId);
        membership.UserId.Should().Be(ownerUserId);
        membership.Role.Should().Be(role);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);
        membership.StartedAt.Should().Be(referenceTime);
        membership.EndedAt.Should().BeNull();
    }

    [Fact]
    public void CreateFamilyMember_ShouldThrowException_WhenPatientIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            FamilyMembership.CreateFamilyMember(
                Guid.Empty,
                Guid.CreateVersion7(),
                PatientRelationship.Spouse,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CreateFamilyMember_ShouldThrowException_WhenOwnerUserIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            FamilyMembership.CreateFamilyMember(
                Guid.CreateVersion7(),
                Guid.Empty,
                PatientRelationship.Sibling,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CreateFamilyMember_ShouldThrowException_WhenReferenceTimeIsDefault()
    {
        // Arrange & Act
        var act = () =>
            FamilyMembership.CreateFamilyMember(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                PatientRelationship.Child,
                default
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CreateFamilyMember_ShouldThrowException_WhenRoleIsInvalid()
    {
        // Arrange & Act
        var act = () =>
            FamilyMembership.CreateFamilyMember(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                (PatientRelationship)999,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void CreateFamilyMember_ShouldThrowException_WhenRoleIsSelf()
    {
        // Arrange & Act
        var act = () =>
            FamilyMembership.CreateFamilyMember(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                PatientRelationship.Self,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.CannotBeSelf);
    }

    [Fact]
    public void Revoke_ShouldTransitionToRevoked_WhenValidParameters()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _fakeTime.Advance(TimeSpan.FromDays(10));
        var revokeTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        membership.Revoke(
            patientHasOwnSelfMembership: true,
            hasUpcomingAppointmentRequiringGuardianForMinor: false,
            referenceTime: revokeTime
        );

        // Assert
        membership.Status.Should().Be(FamilyMembershipStatus.Revoked);
        membership.EndedAt.Should().Be(revokeTime);
    }

    [Fact]
    public void Revoke_ShouldThrowException_WhenRoleIsSelf()
    {
        // Arrange
        var membership = FamilyMembership.CreateSelf(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _fakeTime.Advance(TimeSpan.FromDays(1));
        var revokeTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        var act = () =>
            membership.Revoke(
                patientHasOwnSelfMembership: true,
                hasUpcomingAppointmentRequiringGuardianForMinor: false,
                referenceTime: revokeTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.CannotRemoveSelf);
    }

    [Fact]
    public void Revoke_ShouldThrowException_WhenStatusIsAlreadyRevoked()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        _fakeTime.Advance(TimeSpan.FromDays(1));

        var actionTime = _fakeTime.GetUtcNow().UtcDateTime;
        membership.Revoke(
            patientHasOwnSelfMembership: true,
            hasUpcomingAppointmentRequiringGuardianForMinor: false,
            referenceTime: actionTime
        );

        // Act
        var act = () =>
            membership.Revoke(
                patientHasOwnSelfMembership: true,
                hasUpcomingAppointmentRequiringGuardianForMinor: false,
                referenceTime: actionTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.AlreadyTerminated);
    }

    [Fact]
    public void Revoke_ShouldThrowException_WhenStatusIsAlreadyLeft()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        _fakeTime.Advance(TimeSpan.FromDays(1));

        var actionTime = _fakeTime.GetUtcNow().UtcDateTime;
        membership.Leave(FamilyMembership.MinimumAgeToLeave, actionTime);

        // Act
        var act = () =>
            membership.Revoke(
                patientHasOwnSelfMembership: true,
                hasUpcomingAppointmentRequiringGuardianForMinor: false,
                referenceTime: actionTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.AlreadyTerminated);
    }

    [Fact]
    public void Revoke_ShouldThrowException_WhenPatientHasUpcomingAppointmentRequiringGuardianForMinor()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        _fakeTime.Advance(TimeSpan.FromDays(1));

        var revokeTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        var act = () =>
            membership.Revoke(
                patientHasOwnSelfMembership: true,
                hasUpcomingAppointmentRequiringGuardianForMinor: true,
                referenceTime: revokeTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.CannotRemoveWithUpcomingAppointments);
    }

    [Fact]
    public void Revoke_ShouldThrowException_WhenPatientHasNoOwnSelfMembership()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        _fakeTime.Advance(TimeSpan.FromDays(1));

        var revokeTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        var act = () =>
            membership.Revoke(
                patientHasOwnSelfMembership: false,
                hasUpcomingAppointmentRequiringGuardianForMinor: false,
                referenceTime: revokeTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.CannotRemoveWithoutOwnSelf);
    }

    [Fact]
    public void Revoke_ShouldThrowException_WhenReferenceTimeIsBeforeStartedAt()
    {
        // Arrange
        var startedAt = _fakeTime.GetUtcNow().UtcDateTime;
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            startedAt
        );

        // Act
        var act = () =>
            membership.Revoke(
                patientHasOwnSelfMembership: true,
                hasUpcomingAppointmentRequiringGuardianForMinor: false,
                referenceTime: startedAt.AddSeconds(-1)
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }

    [Fact]
    public void Revoke_ShouldThrowException_WhenReferenceTimeIsEqualToStartedAt()
    {
        // Arrange
        var startedAt = _fakeTime.GetUtcNow().UtcDateTime;
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            startedAt
        );

        // Act
        var act = () =>
            membership.Revoke(
                patientHasOwnSelfMembership: true,
                hasUpcomingAppointmentRequiringGuardianForMinor: false,
                referenceTime: startedAt
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }

    [Fact]
    public void Leave_ShouldTransitionToLeft_WhenStatusIsActive()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _fakeTime.Advance(TimeSpan.FromDays(5));

        var leaveTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        membership.Leave(FamilyMembership.MinimumAgeToLeave, leaveTime);

        // Assert
        membership.Status.Should().Be(FamilyMembershipStatus.Left);
        membership.EndedAt.Should().Be(leaveTime);
    }

    [Fact]
    public void Leave_ShouldThrowException_WhenRoleIsSelf()
    {
        // Arrange
        var membership = FamilyMembership.CreateSelf(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        _fakeTime.Advance(TimeSpan.FromDays(1));

        var actionTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        var act = () => membership.Leave(FamilyMembership.MinimumAgeToLeave, actionTime);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.CannotLeaveSelf);
    }

    [Fact]
    public void Leave_ShouldThrowException_WhenMemberIsUnderage()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        _fakeTime.Advance(TimeSpan.FromDays(1));

        var actionTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        var act = () => membership.Leave(FamilyMembership.MinimumAgeToLeave - 1, actionTime);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.MemberMustBeAdultToLeave);
    }

    [Fact]
    public void Leave_ShouldThrowException_WhenStatusIsAlreadyLeft()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        _fakeTime.Advance(TimeSpan.FromDays(1));

        var actionTime = _fakeTime.GetUtcNow().UtcDateTime;
        membership.Leave(FamilyMembership.MinimumAgeToLeave, actionTime);

        // Act
        var act = () => membership.Leave(FamilyMembership.MinimumAgeToLeave, actionTime);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.AlreadyTerminated);
    }

    [Fact]
    public void Leave_ShouldThrowException_WhenStatusIsAlreadyRevoked()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        _fakeTime.Advance(TimeSpan.FromDays(1));

        var actionTime = _fakeTime.GetUtcNow().UtcDateTime;
        membership.Revoke(
            patientHasOwnSelfMembership: true,
            hasUpcomingAppointmentRequiringGuardianForMinor: false,
            referenceTime: actionTime
        );

        // Act
        var act = () => membership.Leave(FamilyMembership.MinimumAgeToLeave, actionTime);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.AlreadyTerminated);
    }

    [Fact]
    public void Leave_ShouldThrowException_WhenReferenceTimeIsBeforeStartedAt()
    {
        // Arrange
        var startedAt = _fakeTime.GetUtcNow().UtcDateTime;
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            startedAt
        );

        // Act
        var act = () =>
            membership.Leave(FamilyMembership.MinimumAgeToLeave, startedAt.AddSeconds(-1));

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }

    [Fact]
    public void Leave_ShouldThrowException_WhenReferenceTimeIsEqualToStartedAt()
    {
        // Arrange
        var startedAt = _fakeTime.GetUtcNow().UtcDateTime;
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            startedAt
        );

        // Act
        var act = () => membership.Leave(FamilyMembership.MinimumAgeToLeave, startedAt);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }

    [Fact]
    public void CloseSelfMembership_ShouldTransitionToClosed_WhenStatusIsActiveAndRoleIsSelf()
    {
        // Arrange
        var membership = FamilyMembership.CreateSelf(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _fakeTime.Advance(TimeSpan.FromDays(5));

        var closeTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        membership.CloseSelfMembership(closeTime);

        // Assert
        membership.Status.Should().Be(FamilyMembershipStatus.Closed);
        membership.EndedAt.Should().Be(closeTime);
    }

    [Fact]
    public void CloseSelfMembership_ShouldThrowException_WhenRoleIsNotSelf()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            _fakeTime.GetUtcNow().UtcDateTime
        );
        _fakeTime.Advance(TimeSpan.FromDays(1));

        var actionTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        var act = () => membership.CloseSelfMembership(actionTime);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.CanOnlyCloseSelfMembership);
    }

    [Fact]
    public void CloseSelfMembership_ShouldThrowException_WhenStatusIsAlreadyClosed()
    {
        // Arrange
        var membership = FamilyMembership.CreateSelf(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );
        _fakeTime.Advance(TimeSpan.FromDays(1));

        var actionTime = _fakeTime.GetUtcNow().UtcDateTime;
        membership.CloseSelfMembership(actionTime);

        // Act
        var act = () => membership.CloseSelfMembership(actionTime);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.AlreadyTerminated);
    }

    [Fact]
    public void CloseSelfMembership_ShouldThrowException_WhenReferenceTimeIsBeforeStartedAt()
    {
        // Arrange
        var startedAt = _fakeTime.GetUtcNow().UtcDateTime;
        var membership = FamilyMembership.CreateSelf(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            startedAt
        );

        // Act
        var act = () => membership.CloseSelfMembership(startedAt.AddSeconds(-1));

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }

    [Fact]
    public void CloseSelfMembership_ShouldThrowException_WhenReferenceTimeIsEqualToStartedAt()
    {
        // Arrange
        var startedAt = _fakeTime.GetUtcNow().UtcDateTime;
        var membership = FamilyMembership.CreateSelf(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            startedAt
        );

        // Act
        var act = () => membership.CloseSelfMembership(startedAt);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }
}
