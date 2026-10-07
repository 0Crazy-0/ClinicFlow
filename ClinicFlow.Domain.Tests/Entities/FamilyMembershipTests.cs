using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Exceptions.Patients;
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
        membership.LegalAuthority.Should().Be(LegalAuthorityType.None);
        membership.AccessLevel.Should().Be(FamilyMembershipAccessLevel.Full);
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
            FamilyMembership.CreateSelf(Guid.CreateVersion7(), Guid.CreateVersion7(), default);

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
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
            referenceTime
        );

        // Assert
        membership.PatientId.Should().Be(patientId);
        membership.UserId.Should().Be(ownerUserId);
        membership.Role.Should().Be(role);
        membership.AccessLevel.Should().Be(FamilyMembershipAccessLevel.Full);
        membership.LegalAuthority.Should().Be(LegalAuthorityType.None);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);
        membership.StartedAt.Should().Be(referenceTime);
        membership.EndedAt.Should().BeNull();
    }

    [Fact]
    public void CreateFamilyMember_ShouldCreateMembership_WhenParentRoleWithParentAuthorityAndMinor()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var ownerUserId = Guid.CreateVersion7();
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        var membership = FamilyMembership.CreateFamilyMember(
            patientId,
            ownerUserId,
            PatientRelationship.Parent,
            LegalAuthorityType.Parent,
            FamilyMembershipAccessLevel.Full,
            FamilyMembership.MinimumAdultAge - 1,
            referenceTime
        );

        // Assert
        membership.PatientId.Should().Be(patientId);
        membership.UserId.Should().Be(ownerUserId);
        membership.Role.Should().Be(PatientRelationship.Parent);
        membership.AccessLevel.Should().Be(FamilyMembershipAccessLevel.Full);
        membership.LegalAuthority.Should().Be(LegalAuthorityType.Parent);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);
        membership.StartedAt.Should().Be(referenceTime);
        membership.EndedAt.Should().BeNull();
    }

    [Fact]
    public void CreateFamilyMember_ShouldCreateMembership_WhenOtherRoleWithGuardianAuthorityAndMinor()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var ownerUserId = Guid.CreateVersion7();
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        var membership = FamilyMembership.CreateFamilyMember(
            patientId,
            ownerUserId,
            PatientRelationship.Other,
            LegalAuthorityType.Guardian,
            FamilyMembershipAccessLevel.Full,
            FamilyMembership.MinimumAdultAge - 1,
            referenceTime
        );

        // Assert
        membership.PatientId.Should().Be(patientId);
        membership.UserId.Should().Be(ownerUserId);
        membership.Role.Should().Be(PatientRelationship.Other);
        membership.AccessLevel.Should().Be(FamilyMembershipAccessLevel.Full);
        membership.LegalAuthority.Should().Be(LegalAuthorityType.Guardian);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);
        membership.StartedAt.Should().Be(referenceTime);
        membership.EndedAt.Should().BeNull();
    }

    [Fact]
    public void CreateFamilyMember_ShouldCreateMembership_WhenSiblingRoleWithGuardianAuthorityAndMinor()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var ownerUserId = Guid.CreateVersion7();
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        var membership = FamilyMembership.CreateFamilyMember(
            patientId,
            ownerUserId,
            PatientRelationship.Sibling,
            LegalAuthorityType.Guardian,
            FamilyMembershipAccessLevel.Full,
            FamilyMembership.MinimumAdultAge - 1,
            referenceTime
        );

        // Assert
        membership.PatientId.Should().Be(patientId);
        membership.UserId.Should().Be(ownerUserId);
        membership.Role.Should().Be(PatientRelationship.Sibling);
        membership.AccessLevel.Should().Be(FamilyMembershipAccessLevel.Full);
        membership.LegalAuthority.Should().Be(LegalAuthorityType.Guardian);
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
                LegalAuthorityType.None,
                FamilyMembershipAccessLevel.Full,
                30,
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
                LegalAuthorityType.None,
                FamilyMembershipAccessLevel.Full,
                30,
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
                LegalAuthorityType.None,
                FamilyMembershipAccessLevel.Full,
                30,
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
                LegalAuthorityType.None,
                FamilyMembershipAccessLevel.Full,
                30,
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
                LegalAuthorityType.None,
                FamilyMembershipAccessLevel.Full,
                30,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.CannotBeSelf);
    }

    [Fact]
    public void CreateFamilyMember_ShouldThrowException_WhenLegalAuthorityIsInvalid()
    {
        // Arrange & Act
        var act = () =>
            FamilyMembership.CreateFamilyMember(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                PatientRelationship.Parent,
                (LegalAuthorityType)999,
                FamilyMembershipAccessLevel.Full,
                FamilyMembership.MinimumAdultAge - 1,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Theory]
    [InlineData(PatientRelationship.Child)]
    [InlineData(PatientRelationship.Spouse)]
    [InlineData(PatientRelationship.Sibling)]
    [InlineData(PatientRelationship.Other)]
    public void CreateFamilyMember_ShouldThrowException_WhenParentAuthorityWithNonParentRole(
        PatientRelationship role
    )
    {
        // Arrange & Act
        var act = () =>
            FamilyMembership.CreateFamilyMember(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                role,
                LegalAuthorityType.Parent,
                FamilyMembershipAccessLevel.Full,
                FamilyMembership.MinimumAdultAge - 1,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.InvalidLegalAuthorityForRole);
    }

    [Theory]
    [InlineData(PatientRelationship.Parent)]
    [InlineData(PatientRelationship.Child)]
    [InlineData(PatientRelationship.Spouse)]
    public void CreateFamilyMember_ShouldThrowException_WhenGuardianAuthorityWithInvalidRole(
        PatientRelationship role
    )
    {
        // Arrange & Act
        var act = () =>
            FamilyMembership.CreateFamilyMember(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                role,
                LegalAuthorityType.Guardian,
                FamilyMembershipAccessLevel.Full,
                FamilyMembership.MinimumAdultAge - 1,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.InvalidLegalAuthorityForRole);
    }

    [Theory]
    [InlineData(PatientRelationship.Parent, LegalAuthorityType.Parent)]
    [InlineData(PatientRelationship.Other, LegalAuthorityType.Guardian)]
    [InlineData(PatientRelationship.Sibling, LegalAuthorityType.Guardian)]
    public void CreateFamilyMember_ShouldThrowException_WhenLegalAuthorityForAdult(
        PatientRelationship role,
        LegalAuthorityType legalAuthority
    )
    {
        // Arrange & Act
        var act = () =>
            FamilyMembership.CreateFamilyMember(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                role,
                legalAuthority,
                FamilyMembershipAccessLevel.Full,
                FamilyMembership.MinimumAdultAge,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.LegalAuthorityRequiresMinor);
    }

    [Theory]
    [InlineData(PatientRelationship.Parent)]
    [InlineData(PatientRelationship.Other)]
    [InlineData(PatientRelationship.Sibling)]
    [InlineData(PatientRelationship.Child)]
    [InlineData(PatientRelationship.Spouse)]
    public void CreateFamilyMember_ShouldThrowException_WhenMinorWithoutLegalAuthority(
        PatientRelationship role
    )
    {
        // Arrange & Act
        var act = () =>
            FamilyMembership.CreateFamilyMember(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                role,
                LegalAuthorityType.None,
                FamilyMembershipAccessLevel.Full,
                FamilyMembership.MinimumAdultAge - 1,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.MinorRequiresLegalAuthority);
    }

    [Fact]
    public void CreateFamilyMember_ShouldThrowException_WhenAccessLevelIsInvalid()
    {
        // Arrange & Act
        var act = () =>
            FamilyMembership.CreateFamilyMember(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                PatientRelationship.Child,
                LegalAuthorityType.None,
                (FamilyMembershipAccessLevel)999,
                30,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void CreateFamilyMember_ShouldThrowException_WhenAccessLevelIsUnspecified()
    {
        // Arrange & Act
        var act = () =>
            FamilyMembership.CreateFamilyMember(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                PatientRelationship.Child,
                LegalAuthorityType.None,
                FamilyMembershipAccessLevel.Unspecified,
                30,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(FamilyMembershipAccessLevel.ViewOnly)]
    [InlineData(FamilyMembershipAccessLevel.Restricted)]
    [InlineData(FamilyMembershipAccessLevel.EmergencyOnly)]
    [InlineData(FamilyMembershipAccessLevel.AppointmentOnly)]
    public void CreateFamilyMember_ShouldThrowException_WhenPatientIsMinorAndAccessLevelIsNotFull(
        FamilyMembershipAccessLevel accessLevel
    )
    {
        // Arrange & Act
        var act = () =>
            FamilyMembership.CreateFamilyMember(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                PatientRelationship.Parent,
                LegalAuthorityType.Parent,
                accessLevel,
                patientAge: FamilyMembership.MinimumAdultAge - 1,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.MinorMustHaveFullAccess);
    }

    [Fact]
    public void CreateFamilyMember_ShouldCreateMembership_WhenPatientIsMinorAndAccessLevelIsFull()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var ownerUserId = Guid.CreateVersion7();
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        var membership = FamilyMembership.CreateFamilyMember(
            patientId,
            ownerUserId,
            PatientRelationship.Parent,
            LegalAuthorityType.Parent,
            FamilyMembershipAccessLevel.Full,
            FamilyMembership.MinimumAdultAge - 1,
            referenceTime
        );

        // Assert
        membership.PatientId.Should().Be(patientId);
        membership.UserId.Should().Be(ownerUserId);
        membership.Role.Should().Be(PatientRelationship.Parent);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);
        membership.AccessLevel.Should().Be(FamilyMembershipAccessLevel.Full);
        membership.StartedAt.Should().Be(referenceTime);
        membership.EndedAt.Should().BeNull();
    }

    [Fact]
    public void CreateFamilyMember_ShouldCreateMembership_WhenPatientIsExactlyAdultAge()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var ownerUserId = Guid.CreateVersion7();
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        var membership = FamilyMembership.CreateFamilyMember(
            patientId,
            ownerUserId,
            PatientRelationship.Sibling,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Restricted,
            FamilyMembership.MinimumAdultAge,
            referenceTime
        );

        // Assert
        membership.PatientId.Should().Be(patientId);
        membership.UserId.Should().Be(ownerUserId);
        membership.Role.Should().Be(PatientRelationship.Sibling);
        membership.Status.Should().Be(FamilyMembershipStatus.Active);
        membership.AccessLevel.Should().Be(FamilyMembershipAccessLevel.Restricted);
        membership.StartedAt.Should().Be(referenceTime);
        membership.EndedAt.Should().BeNull();
    }

    [Fact]
    public void ChangeAccessLevel_ShouldThrowException_WhenAccessLevelIsInvalid()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () =>
            membership.ChangeAccessLevel(
                (FamilyMembershipAccessLevel)999,
                30,
                requesterIsAuthorized: true
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void ChangeAccessLevel_ShouldThrowException_WhenNewAccessLevelIsUnspecified()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () =>
            membership.ChangeAccessLevel(
                FamilyMembershipAccessLevel.Unspecified,
                30,
                requesterIsAuthorized: true
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void ChangeAccessLevel_ShouldThrowException_WhenNewAccessLevelIsDefault()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () => membership.ChangeAccessLevel(default, 30, requesterIsAuthorized: true);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void ChangeAccessLevel_ShouldUpdateAccessLevel_WhenRequesterIsAuthorizedAndAccessLevelIsDifferent()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        membership.ChangeAccessLevel(
            FamilyMembershipAccessLevel.ViewOnly,
            30,
            requesterIsAuthorized: true
        );

        // Assert
        membership.AccessLevel.Should().Be(FamilyMembershipAccessLevel.ViewOnly);
    }

    [Theory]
    [InlineData(FamilyMembershipAccessLevel.Full)]
    [InlineData(FamilyMembershipAccessLevel.ViewOnly)]
    [InlineData(FamilyMembershipAccessLevel.EmergencyOnly)]
    [InlineData(FamilyMembershipAccessLevel.AppointmentOnly)]
    public void ChangeAccessLevel_ShouldUpdateAccessLevel_WhenChangingToRestricted(
        FamilyMembershipAccessLevel currentAccessLevel
    )
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            currentAccessLevel,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        membership.ChangeAccessLevel(
            FamilyMembershipAccessLevel.Restricted,
            30,
            requesterIsAuthorized: true
        );

        // Assert
        membership.AccessLevel.Should().Be(FamilyMembershipAccessLevel.Restricted);
    }

    [Fact]
    public void ChangeAccessLevel_ShouldThrowException_WhenRoleIsSelf()
    {
        // Arrange
        var membership = FamilyMembership.CreateSelf(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () =>
            membership.ChangeAccessLevel(
                FamilyMembershipAccessLevel.ViewOnly,
                30,
                requesterIsAuthorized: true
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.CannotChangeAccessLevelOfSelf);
    }

    [Theory]
    [InlineData(FamilyMembershipAccessLevel.Full)]
    [InlineData(FamilyMembershipAccessLevel.ViewOnly)]
    [InlineData(FamilyMembershipAccessLevel.Restricted)]
    [InlineData(FamilyMembershipAccessLevel.EmergencyOnly)]
    [InlineData(FamilyMembershipAccessLevel.AppointmentOnly)]
    public void ChangeAccessLevel_ShouldThrowException_WhenPatientIsMinor(
        FamilyMembershipAccessLevel newAccessLevel
    )
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Parent,
            LegalAuthorityType.Parent,
            FamilyMembershipAccessLevel.Full,
            patientAge: FamilyMembership.MinimumAdultAge - 1,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () =>
            membership.ChangeAccessLevel(
                newAccessLevel,
                FamilyMembership.MinimumAdultAge - 1,
                requesterIsAuthorized: true
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.CannotChangeAccessLevelWhileMinor);
    }

    [Fact]
    public void ChangeAccessLevel_ShouldUpdateAccessLevel_WhenPatientIsExactlyAdultAge()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Sibling,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            FamilyMembership.MinimumAdultAge,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        membership.ChangeAccessLevel(
            FamilyMembershipAccessLevel.ViewOnly,
            FamilyMembership.MinimumAdultAge,
            requesterIsAuthorized: true
        );

        // Assert
        membership.AccessLevel.Should().Be(FamilyMembershipAccessLevel.ViewOnly);
    }

    [Fact]
    public void ChangeAccessLevel_ShouldThrowException_WhenRequesterIsNotAuthorized()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () =>
            membership.ChangeAccessLevel(
                FamilyMembershipAccessLevel.ViewOnly,
                30,
                requesterIsAuthorized: false
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.UnauthorizedAccessLevelChange);
    }

    [Fact]
    public void ChangeAccessLevel_ShouldThrowException_WhenAccessLevelIsUnchanged()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.ViewOnly,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () =>
            membership.ChangeAccessLevel(
                FamilyMembershipAccessLevel.ViewOnly,
                30,
                requesterIsAuthorized: true
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.AccessLevelUnchanged);
    }

    [Theory]
    [InlineData(FamilyMembershipAccessLevel.Full)]
    [InlineData(FamilyMembershipAccessLevel.ViewOnly)]
    [InlineData(FamilyMembershipAccessLevel.EmergencyOnly)]
    [InlineData(FamilyMembershipAccessLevel.AppointmentOnly)]
    public void ChangeAccessLevel_ShouldThrowException_WhenChangingFromRestrictedWithCategoriesConfigured(
        FamilyMembershipAccessLevel newAccessLevel
    )
    {
        // Arrange
        var membership = CreateRestrictedMembership();
        membership.AddAllowedAppointmentCategory(
            AppointmentCategory.Pediatrics,
            requesterIsAuthorized: true
        );

        // Act
        var act = () =>
            membership.ChangeAccessLevel(newAccessLevel, 30, requesterIsAuthorized: true);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(
                DomainErrors.FamilyMembership.CannotChangeAccessLevelWithCategoriesConfigured
            );
    }

    [Theory]
    [InlineData(FamilyMembershipAccessLevel.Full)]
    [InlineData(FamilyMembershipAccessLevel.ViewOnly)]
    public void ChangeAccessLevel_ShouldUpdateAccessLevel_WhenChangingFromRestrictedWithEmptyCategoryList(
        FamilyMembershipAccessLevel newAccessLevel
    )
    {
        // Arrange
        var membership = CreateRestrictedMembership();

        // Act
        membership.ChangeAccessLevel(newAccessLevel, 30, requesterIsAuthorized: true);

        // Assert
        membership.AccessLevel.Should().Be(newAccessLevel);
    }

    [Fact]
    public void AddAllowedAppointmentCategory_ShouldAddCategory_WhenAccessLevelIsRestricted()
    {
        // Arrange
        var membership = CreateRestrictedMembership();

        // Act
        membership.AddAllowedAppointmentCategory(
            AppointmentCategory.Pediatrics,
            requesterIsAuthorized: true
        );

        // Assert
        membership
            .AllowedAppointmentCategories.Should()
            .ContainSingle()
            .Which.Should()
            .Be(AppointmentCategory.Pediatrics);
    }

    [Fact]
    public void AddAllowedAppointmentCategory_ShouldThrowException_WhenRequesterIsNotAuthorized()
    {
        // Arrange
        var membership = CreateRestrictedMembership();

        // Act
        var act = () =>
            membership.AddAllowedAppointmentCategory(
                AppointmentCategory.Pediatrics,
                requesterIsAuthorized: false
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.UnauthorizedCategoryListChange);
    }

    [Theory]
    [InlineData(FamilyMembershipAccessLevel.Full)]
    [InlineData(FamilyMembershipAccessLevel.ViewOnly)]
    [InlineData(FamilyMembershipAccessLevel.EmergencyOnly)]
    [InlineData(FamilyMembershipAccessLevel.AppointmentOnly)]
    public void AddAllowedAppointmentCategory_ShouldThrowException_WhenAccessLevelIsNotRestricted(
        FamilyMembershipAccessLevel accessLevel
    )
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            accessLevel,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () =>
            membership.AddAllowedAppointmentCategory(
                AppointmentCategory.Pediatrics,
                requesterIsAuthorized: true
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.OnlyRestrictedCanHaveCategoryList);
    }

    [Fact]
    public void AddAllowedAppointmentCategory_ShouldThrowException_WhenCategoryIsNotDefined()
    {
        // Arrange
        var membership = CreateRestrictedMembership();

        // Act
        var act = () =>
            membership.AddAllowedAppointmentCategory(
                (AppointmentCategory)999,
                requesterIsAuthorized: true
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void AddAllowedAppointmentCategory_ShouldThrowException_WhenCategoryIsAlreadyAllowed()
    {
        // Arrange
        var membership = CreateRestrictedMembership();
        membership.AddAllowedAppointmentCategory(
            AppointmentCategory.Pediatrics,
            requesterIsAuthorized: true
        );

        // Act
        var act = () =>
            membership.AddAllowedAppointmentCategory(
                AppointmentCategory.Pediatrics,
                requesterIsAuthorized: true
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.CategoryAlreadyAllowed);
    }

    [Fact]
    public void RemoveAllowedAppointmentCategory_ShouldRemoveCategory_WhenCategoryIsAllowed()
    {
        // Arrange
        var membership = CreateRestrictedMembership();
        membership.AddAllowedAppointmentCategory(
            AppointmentCategory.Pediatrics,
            requesterIsAuthorized: true
        );

        // Act
        membership.RemoveAllowedAppointmentCategory(
            AppointmentCategory.Pediatrics,
            requesterIsAuthorized: true
        );

        // Assert
        membership.AllowedAppointmentCategories.Should().BeEmpty();
    }

    [Fact]
    public void RemoveAllowedAppointmentCategory_ShouldThrowException_WhenRequesterIsNotAuthorized()
    {
        // Arrange
        var membership = CreateRestrictedMembership();

        // Act
        var act = () =>
            membership.RemoveAllowedAppointmentCategory(
                AppointmentCategory.Pediatrics,
                requesterIsAuthorized: false
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.UnauthorizedCategoryListChange);
    }

    [Theory]
    [InlineData(FamilyMembershipAccessLevel.Full)]
    [InlineData(FamilyMembershipAccessLevel.ViewOnly)]
    [InlineData(FamilyMembershipAccessLevel.EmergencyOnly)]
    [InlineData(FamilyMembershipAccessLevel.AppointmentOnly)]
    public void RemoveAllowedAppointmentCategory_ShouldThrowException_WhenAccessLevelIsNotRestricted(
        FamilyMembershipAccessLevel accessLevel
    )
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            accessLevel,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () =>
            membership.RemoveAllowedAppointmentCategory(
                AppointmentCategory.Pediatrics,
                requesterIsAuthorized: true
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.OnlyRestrictedCanHaveCategoryList);
    }

    [Fact]
    public void RemoveAllowedAppointmentCategory_ShouldThrowException_WhenCategoryIsNotAllowed()
    {
        // Arrange
        var membership = CreateRestrictedMembership();

        // Act
        var act = () =>
            membership.RemoveAllowedAppointmentCategory(
                AppointmentCategory.Pediatrics,
                requesterIsAuthorized: true
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.FamilyMembership.CategoryNotFound);
    }

    [Theory]
    [InlineData(FamilyMembershipAccessLevel.Full)]
    [InlineData(FamilyMembershipAccessLevel.ViewOnly)]
    public void EnsureMedicalRecordsAccess_ShouldNotThrow_WhenAccessLevelAllowsClinicalDataAccess(
        FamilyMembershipAccessLevel accessLevel
    )
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            accessLevel,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act & Assert
        membership.Invoking(m => m.EnsureMedicalRecordsAccess()).Should().NotThrow();
    }

    [Theory]
    [InlineData(FamilyMembershipAccessLevel.Restricted)]
    [InlineData(FamilyMembershipAccessLevel.EmergencyOnly)]
    [InlineData(FamilyMembershipAccessLevel.AppointmentOnly)]
    public void EnsureMedicalRecordsAccess_ShouldThrowException_WhenAccessLevelDeniesClinicalDataAccess(
        FamilyMembershipAccessLevel accessLevel
    )
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            accessLevel,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act & Assert
        membership
            .Invoking(m => m.EnsureMedicalRecordsAccess())
            .Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecord.UnauthorizedAccess);
    }

    [Fact]
    public void EnsureAppointmentAccess_ShouldNotThrow_WhenRoleIsSelf()
    {
        // Arrange
        var membership = FamilyMembership.CreateSelf(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act & Assert
        membership
            .Invoking(m => m.EnsureAppointmentAccess(AppointmentCategory.GeneralMedicine))
            .Should()
            .NotThrow();
    }

    [Fact]
    public void EnsureAppointmentAccess_ShouldNotThrow_WhenFullAccess()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act & Assert
        membership
            .Invoking(m => m.EnsureAppointmentAccess(AppointmentCategory.Cardiology))
            .Should()
            .NotThrow();
    }

    [Fact]
    public void EnsureAppointmentAccess_ShouldNotThrow_WhenAppointmentOnlyAccess()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.AppointmentOnly,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act & Assert
        membership
            .Invoking(m => m.EnsureAppointmentAccess(AppointmentCategory.Dermatology))
            .Should()
            .NotThrow();
    }

    [Fact]
    public void EnsureAppointmentAccess_ShouldNotThrow_WhenRestrictedWithAllowedCategory()
    {
        // Arrange
        var membership = CreateRestrictedMembership();

        membership.AddAllowedAppointmentCategory(
            AppointmentCategory.GeneralMedicine,
            requesterIsAuthorized: true
        );

        // Act & Assert
        membership
            .Invoking(m => m.EnsureAppointmentAccess(AppointmentCategory.GeneralMedicine))
            .Should()
            .NotThrow();
    }

    [Fact]
    public void EnsureAppointmentAccess_ShouldThrowPatientAccessUnauthorizedException_WhenViewOnly()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.ViewOnly,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () => membership.EnsureAppointmentAccess(AppointmentCategory.GeneralMedicine);

        // Assert
        act.Should()
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);
    }

    [Fact]
    public void EnsureAppointmentAccess_ShouldThrowPatientAccessUnauthorizedException_WhenRestrictedWithoutCategory()
    {
        // Arrange
        var membership = CreateRestrictedMembership();

        // Act
        var act = () => membership.EnsureAppointmentAccess(AppointmentCategory.Cardiology);

        // Assert
        act.Should()
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);
    }

    [Fact]
    public void GetVisibleAppointmentCategories_ShouldReturnAll_WhenFullAccess()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var visible = membership.GetVisibleAppointmentCategories();

        // Assert
        visible.Should().BeEquivalentTo(Enum.GetValues<AppointmentCategory>());
    }

    [Fact]
    public void GetVisibleAppointmentCategories_ShouldReturnAll_WhenViewOnlyAccess()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.ViewOnly,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var visible = membership.GetVisibleAppointmentCategories();

        // Assert
        visible.Should().BeEquivalentTo(Enum.GetValues<AppointmentCategory>());
    }

    [Fact]
    public void GetVisibleAppointmentCategories_ShouldReturnAllowedOnly_WhenRestricted()
    {
        // Arrange
        var membership = CreateRestrictedMembership();

        membership.AddAllowedAppointmentCategory(
            AppointmentCategory.GeneralMedicine,
            requesterIsAuthorized: true
        );

        // Act
        var visible = membership.GetVisibleAppointmentCategories();

        // Assert
        visible.Should().BeEquivalentTo([AppointmentCategory.GeneralMedicine]);
    }

    [Fact]
    public void GetVisibleAppointmentCategories_ShouldThrowDomainValidationException_WhenAppointmentOnly()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.AppointmentOnly,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () => membership.GetVisibleAppointmentCategories();

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Appointment.UnauthorizedAccess);
    }

    [Fact]
    public void Revoke_ShouldTransitionToRevoked_WhenValidParameters()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
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
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
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
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
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
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
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
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
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
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
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
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
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
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
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
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
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
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
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
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
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
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
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
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
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
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
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

    [Fact]
    public void EnsureGuardianAccess_ShouldNotThrow_WhenFullParentActsAsGuardian()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var membership = CreateParentMembershipForMinor(userId);

        // Act & Assert
        membership.Invoking(m => m.EnsureGuardianAccess()).Should().NotThrow();
    }

    [Fact]
    public void EnsureGuardianAccess_ShouldThrowUnauthorized_WhenSelfAttempts()
    {
        // Arrange
        var membership = FamilyMembership.CreateSelf(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act & Assert
        membership
            .Invoking(m => m.EnsureGuardianAccess())
            .Should()
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);
    }

    [Fact]
    public void EnsureGuardianAccess_ShouldThrowUnauthorized_WhenAuthorityIsNone()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Spouse,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act & Assert
        membership
            .Invoking(m => m.EnsureGuardianAccess())
            .Should()
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);
    }

    [Fact]
    public void EnsureGuardianNotesEditAccess_ShouldNotThrow_WhenFullParentAuthorEditsOwnNote()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var membership = CreateParentMembershipForMinor(userId);

        // Act
        var act = () => membership.EnsureGuardianNotesEditAccess(userId);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureGuardianNotesEditAccess_ShouldThrowUnauthorized_WhenDifferentAuthorEdits()
    {
        // Arrange
        var membership = CreateParentMembershipForMinor(Guid.CreateVersion7());

        // Act
        var act = () => membership.EnsureGuardianNotesEditAccess(Guid.CreateVersion7());

        // Assert
        act.Should()
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);
    }

    [Fact]
    public void EnsureGuardianNotesEditAccess_ShouldThrowUnauthorized_WhenSelfAttempts()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var membership = FamilyMembership.CreateSelf(
            Guid.CreateVersion7(),
            userId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () => membership.EnsureGuardianNotesEditAccess(userId);

        // Assert
        act.Should()
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);
    }

    [Fact]
    public void EnsureGuardianNotesEditAccess_ShouldThrowValidation_WhenAuthorIsEmpty()
    {
        // Arrange
        var membership = CreateParentMembershipForMinor(Guid.CreateVersion7());

        // Act
        var act = () => membership.EnsureGuardianNotesEditAccess(Guid.Empty);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void EnsurePatientNotesWrite_ShouldPass_WhenSelf()
    {
        // Arrange
        var membership = FamilyMembership.CreateSelf(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act & Assert
        membership.Invoking(m => m.EnsurePatientNotesWrite()).Should().NotThrow();
    }

    [Fact]
    public void EnsurePatientNotesWrite_ShouldThrowUnauthorized_WhenGuardianAttempts()
    {
        // Arrange
        var membership = CreateParentMembershipForMinor(Guid.CreateVersion7());

        // Act & Assert
        membership
            .Invoking(m => m.EnsurePatientNotesWrite())
            .Should()
            .Throw<PatientAccessUnauthorizedException>()
            .WithMessage(DomainErrors.Patient.UnauthorizedAccess);
    }

    [Fact]
    public void EnsureConsentGrantSigner_ShouldNotThrow_WhenActiveSelfMembershipMatches()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var membership = FamilyMembership.CreateSelf(
            patientId,
            userId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act & Assert
        membership
            .Invoking(m => m.EnsureConsentGrantSigner(userId, patientId))
            .Should()
            .NotThrow();
    }

    [Fact]
    public void EnsureConsentGrantSigner_ShouldThrowException_WhenRequesterUserIdIsEmpty()
    {
        // Arrange
        var membership = FamilyMembership.CreateSelf(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () => membership.EnsureConsentGrantSigner(Guid.Empty, Guid.CreateVersion7());

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void EnsureConsentGrantSigner_ShouldThrowException_WhenPatientIdIsEmpty()
    {
        // Arrange
        var membership = FamilyMembership.CreateSelf(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () => membership.EnsureConsentGrantSigner(Guid.CreateVersion7(), Guid.Empty);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void EnsureConsentGrantSigner_ShouldThrowException_WhenStatusIsNotActive()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var membership = FamilyMembership.CreateSelf(
            patientId,
            userId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _fakeTime.Advance(TimeSpan.FromDays(1));
        membership.CloseSelfMembership(_fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var act = () => membership.EnsureConsentGrantSigner(userId, patientId);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecordConsentGrant.UnauthorizedCreation);
    }

    [Fact]
    public void EnsureConsentGrantSigner_ShouldThrowException_WhenPatientIdMismatches()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var membership = FamilyMembership.CreateSelf(
            patientId,
            userId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () => membership.EnsureConsentGrantSigner(userId, Guid.CreateVersion7());

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecordConsentGrant.UnauthorizedCreation);
    }

    [Fact]
    public void EnsureConsentGrantSigner_ShouldThrowException_WhenRoleIsNotSelf()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var membership = FamilyMembership.CreateFamilyMember(
            patientId,
            userId,
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () => membership.EnsureConsentGrantSigner(userId, patientId);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecordConsentGrant.UnauthorizedCreation);
    }

    [Fact]
    public void EnsureConsentGrantSigner_ShouldThrowException_WhenRequesterUserIdMismatches()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var membership = FamilyMembership.CreateSelf(
            patientId,
            userId,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () => membership.EnsureConsentGrantSigner(Guid.CreateVersion7(), patientId);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecordConsentGrant.UnauthorizedCreation);
    }

    [Fact]
    public void EnsureConsentGrantRecipient_ShouldNotThrow_WhenActiveFamilyMemberMatches()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var membership = FamilyMembership.CreateFamilyMember(
            patientId,
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act & Assert
        membership.Invoking(m => m.EnsureConsentGrantRecipient(patientId)).Should().NotThrow();
    }

    [Fact]
    public void EnsureConsentGrantRecipient_ShouldThrowException_WhenPatientIdIsEmpty()
    {
        // Arrange
        var membership = CreateRestrictedMembership();

        // Act
        var act = () => membership.EnsureConsentGrantRecipient(Guid.Empty);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void EnsureConsentGrantRecipient_ShouldThrowException_WhenStatusIsNotActive()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var membership = FamilyMembership.CreateFamilyMember(
            patientId,
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        _fakeTime.Advance(TimeSpan.FromDays(1));

        membership.Revoke(
            patientHasOwnSelfMembership: true,
            hasUpcomingAppointmentRequiringGuardianForMinor: false,
            referenceTime: _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () => membership.EnsureConsentGrantRecipient(patientId);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecordConsentGrant.RecipientMustBeActive);
    }

    [Fact]
    public void EnsureConsentGrantRecipient_ShouldThrowException_WhenPatientIdMismatches()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () => membership.EnsureConsentGrantRecipient(Guid.CreateVersion7());

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecordConsentGrant.RecipientPatientMismatch);
    }

    [Fact]
    public void EnsureConsentGrantRecipient_ShouldThrowException_WhenRoleIsSelf()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var membership = FamilyMembership.CreateSelf(
            patientId,
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );

        // Act
        var act = () => membership.EnsureConsentGrantRecipient(patientId);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.MedicalRecordConsentGrant.RecipientCannotBeSelf);
    }

    private FamilyMembership CreateParentMembershipForMinor(Guid userId) =>
        FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            userId,
            PatientRelationship.Parent,
            LegalAuthorityType.Parent,
            FamilyMembershipAccessLevel.Full,
            10,
            _fakeTime.GetUtcNow().UtcDateTime
        );

    private FamilyMembership CreateRestrictedMembership() =>
        FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Restricted,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );
}
