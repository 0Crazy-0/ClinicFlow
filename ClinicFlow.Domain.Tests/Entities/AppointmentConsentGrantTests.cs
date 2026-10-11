using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Entities;

public class AppointmentConsentGrantTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void CreateForCategory_ShouldCreateGrant_WhenValidParameters()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var recipientMembershipId = Guid.CreateVersion7();
        var category = ProtectedCategory.MentalHealthCounseling;
        var signedByUserId = Guid.CreateVersion7();
        var signedAt = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        var grant = AppointmentConsentGrant.CreateForCategory(
            patientId,
            recipientMembershipId,
            category,
            signedByUserId,
            signedAt
        );

        // Assert
        grant.PatientId.Should().Be(patientId);
        grant.RecipientMembershipId.Should().Be(recipientMembershipId);
        grant.Scope.Should().Be(AppointmentConsentScope.Category);
        grant.ProtectedCareCategory.Should().Be(category);
        grant.AppointmentId.Should().BeNull();
        grant.SignedByUserId.Should().Be(signedByUserId);
        grant.SignedAt.Should().Be(signedAt);
        grant.ExpiresAt.Should().Be(signedAt.AddYears(AppointmentConsentGrant.ValidityYears));
        grant.RevokedAt.Should().BeNull();
    }

    [Fact]
    public void CreateForCategory_ShouldThrowException_WhenPatientIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            AppointmentConsentGrant.CreateForCategory(
                Guid.Empty,
                Guid.CreateVersion7(),
                ProtectedCategory.MentalHealthCounseling,
                Guid.CreateVersion7(),
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CreateForCategory_ShouldThrowException_WhenRecipientMembershipIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            AppointmentConsentGrant.CreateForCategory(
                Guid.CreateVersion7(),
                Guid.Empty,
                ProtectedCategory.MentalHealthCounseling,
                Guid.CreateVersion7(),
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CreateForCategory_ShouldThrowException_WhenCategoryIsNotDefined()
    {
        // Arrange & Act
        var act = () =>
            AppointmentConsentGrant.CreateForCategory(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                (ProtectedCategory)967,
                Guid.CreateVersion7(),
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void CreateForCategory_ShouldThrowException_WhenSignedByUserIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            AppointmentConsentGrant.CreateForCategory(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                ProtectedCategory.MentalHealthCounseling,
                Guid.Empty,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CreateForCategory_ShouldThrowException_WhenSignedAtIsDefault()
    {
        // Arrange & Act
        var act = () =>
            AppointmentConsentGrant.CreateForCategory(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                ProtectedCategory.MentalHealthCounseling,
                Guid.CreateVersion7(),
                default
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CreateForAppointment_ShouldCreateGrant_WhenValidParameters()
    {
        // Arrange
        var patientId = Guid.CreateVersion7();
        var recipientMembershipId = Guid.CreateVersion7();
        var appointmentId = Guid.CreateVersion7();
        var signedByUserId = Guid.CreateVersion7();
        var signedAt = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        var grant = AppointmentConsentGrant.CreateForAppointment(
            patientId,
            recipientMembershipId,
            appointmentId,
            signedByUserId,
            signedAt
        );

        // Assert
        grant.PatientId.Should().Be(patientId);
        grant.RecipientMembershipId.Should().Be(recipientMembershipId);
        grant.Scope.Should().Be(AppointmentConsentScope.SpecificAppointment);
        grant.AppointmentId.Should().Be(appointmentId);
        grant.ProtectedCareCategory.Should().BeNull();
        grant.SignedByUserId.Should().Be(signedByUserId);
        grant.SignedAt.Should().Be(signedAt);
        grant.ExpiresAt.Should().Be(signedAt.AddYears(AppointmentConsentGrant.ValidityYears));
        grant.RevokedAt.Should().BeNull();
    }

    [Fact]
    public void CreateForAppointment_ShouldThrowException_WhenPatientIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            AppointmentConsentGrant.CreateForAppointment(
                Guid.Empty,
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CreateForAppointment_ShouldThrowException_WhenRecipientMembershipIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            AppointmentConsentGrant.CreateForAppointment(
                Guid.CreateVersion7(),
                Guid.Empty,
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CreateForAppointment_ShouldThrowException_WhenAppointmentIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            AppointmentConsentGrant.CreateForAppointment(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
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
    public void CreateForAppointment_ShouldThrowException_WhenSignedByUserIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            AppointmentConsentGrant.CreateForAppointment(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
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
    public void CreateForAppointment_ShouldThrowException_WhenSignedAtIsDefault()
    {
        // Arrange & Act
        var act = () =>
            AppointmentConsentGrant.CreateForAppointment(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
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
    public void IsEffectiveAt_ShouldThrowException_WhenReferenceTimeIsDefault()
    {
        // Arrange
        var grant = CreateCategoryGrant();

        // Act
        var act = () => grant.IsEffectiveAt(default);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void IsEffectiveAt_ShouldReturnTrue_WhenNotRevokedAndBeforeExpiry()
    {
        // Arrange
        var grant = CreateCategoryGrant();

        // Act
        var result = grant.IsEffectiveAt(grant.SignedAt.AddDays(10));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsEffectiveAt_ShouldReturnFalse_WhenRevokedEvenBeforeExpiry()
    {
        // Arrange
        var grant = CreateCategoryGrant();

        _fakeTime.Advance(TimeSpan.FromDays(10));

        var revokeTime = _fakeTime.GetUtcNow().UtcDateTime;

        grant.Revoke(requesterIsAuthorized: true, referenceTime: revokeTime);

        // Act
        var result = grant.IsEffectiveAt(revokeTime);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEffectiveAt_ShouldReturnFalse_WhenExpiredEvenIfNotRevoked()
    {
        // Arrange
        var grant = CreateCategoryGrant();

        // Act
        var result = grant.IsEffectiveAt(grant.ExpiresAt.AddDays(1));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEffectiveAt_ShouldReturnFalse_WhenReferenceTimeEqualsExpiry()
    {
        // Arrange
        var grant = CreateCategoryGrant();

        // Act
        var result = grant.IsEffectiveAt(grant.ExpiresAt);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Revoke_ShouldThrowException_WhenReferenceTimeIsDefault()
    {
        // Arrange
        var grant = CreateCategoryGrant();

        // Act
        var act = () => grant.Revoke(requesterIsAuthorized: true, referenceTime: default);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Revoke_ShouldSetRevokedAt_WhenAuthorizedAndInForce()
    {
        // Arrange
        var grant = CreateCategoryGrant();

        _fakeTime.Advance(TimeSpan.FromDays(10));

        var revokeTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        grant.Revoke(requesterIsAuthorized: true, referenceTime: revokeTime);

        // Assert
        grant.RevokedAt.Should().Be(revokeTime);
    }

    [Fact]
    public void Revoke_ShouldThrowException_WhenRequesterIsNotAuthorized()
    {
        // Arrange
        var grant = CreateCategoryGrant();

        var revokeTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        var act = () => grant.Revoke(requesterIsAuthorized: false, referenceTime: revokeTime);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentConsentGrant.UnauthorizedRevocation);
    }

    [Fact]
    public void Revoke_ShouldThrowException_WhenAlreadyRevoked()
    {
        // Arrange
        var grant = CreateCategoryGrant();

        _fakeTime.Advance(TimeSpan.FromDays(10));

        var revokeTime = _fakeTime.GetUtcNow().UtcDateTime;

        grant.Revoke(requesterIsAuthorized: true, referenceTime: revokeTime);

        // Act
        var act = () => grant.Revoke(requesterIsAuthorized: true, referenceTime: revokeTime);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentConsentGrant.AlreadyRevoked);
    }

    [Fact]
    public void Revoke_ShouldThrowException_WhenAlreadyExpired()
    {
        // Arrange
        var grant = CreateCategoryGrant();

        // Act
        var act = () =>
            grant.Revoke(requesterIsAuthorized: true, referenceTime: grant.ExpiresAt.AddDays(1));

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentConsentGrant.AlreadyExpired);
    }

    [Fact]
    public void Revoke_ShouldThrowException_WhenReferenceTimeEqualsExpiry()
    {
        // Arrange
        var grant = CreateCategoryGrant();

        // Act
        var act = () => grant.Revoke(requesterIsAuthorized: true, referenceTime: grant.ExpiresAt);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.AppointmentConsentGrant.AlreadyExpired);
    }

    [Fact]
    public void Revoke_ShouldThrowException_WhenReferenceTimeEqualsSignedAt()
    {
        // Arrange
        var grant = CreateCategoryGrant();

        // Act
        var act = () => grant.Revoke(requesterIsAuthorized: true, referenceTime: grant.SignedAt);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }

    [Fact]
    public void Revoke_ShouldThrowException_WhenReferenceTimeIsBeforeSignedAt()
    {
        // Arrange
        var grant = CreateCategoryGrant();

        // Act
        var act = () =>
            grant.Revoke(requesterIsAuthorized: true, referenceTime: grant.SignedAt.AddSeconds(-1));

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.EndTimeMustBeAfterStartTime);
    }

    private AppointmentConsentGrant CreateCategoryGrant() =>
        AppointmentConsentGrant.CreateForCategory(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            ProtectedCategory.MentalHealthCounseling,
            Guid.CreateVersion7(),
            _fakeTime.GetUtcNow().UtcDateTime
        );
}
