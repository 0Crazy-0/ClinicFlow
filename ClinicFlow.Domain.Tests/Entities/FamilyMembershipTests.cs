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
            FamilyMembershipAccessLevel.Full,
            30,
            referenceTime
        );

        // Assert
        membership.PatientId.Should().Be(patientId);
        membership.UserId.Should().Be(ownerUserId);
        membership.Role.Should().Be(role);
        membership.AccessLevel.Should().Be(FamilyMembershipAccessLevel.Full);
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
    public void CreateFamilyMember_ShouldThrowException_WhenAccessLevelIsInvalid()
    {
        // Arrange & Act
        var act = () =>
            FamilyMembership.CreateFamilyMember(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                PatientRelationship.Child,
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
                PatientRelationship.Child,
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
            PatientRelationship.Child,
            FamilyMembershipAccessLevel.Full,
            FamilyMembership.MinimumAdultAge - 1,
            referenceTime
        );

        // Assert
        membership.PatientId.Should().Be(patientId);
        membership.UserId.Should().Be(ownerUserId);
        membership.Role.Should().Be(PatientRelationship.Child);
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
            PatientRelationship.Child,
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
    public void Revoke_ShouldTransitionToRevoked_WhenValidParameters()
    {
        // Arrange
        var membership = FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
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

    private FamilyMembership CreateRestrictedMembership() =>
        FamilyMembership.CreateFamilyMember(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            PatientRelationship.Child,
            FamilyMembershipAccessLevel.Restricted,
            30,
            _fakeTime.GetUtcNow().UtcDateTime
        );
}
