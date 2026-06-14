using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Services;

public class UserAuthenticationServiceTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void TryAuthenticate_ShouldThrowDomainValidationException_WhenUserIsNull()
    {
        // Arrange
        var loginTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        var act = () => UserAuthenticationService.TryAuthenticate(null!, true, loginTime);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void TryAuthenticate_ShouldReturnTrueAndRecordLogin_WhenPasswordIsValid()
    {
        // Arrange
        var user = CreateUser();
        var loginTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        var result = UserAuthenticationService.TryAuthenticate(user, true, loginTime);

        // Assert
        result.Should().BeTrue();
        user.LastLoginAt.Should().Be(loginTime);
        user.FailedLoginAttempts.Should().Be(0);
    }

    [Fact]
    public void TryAuthenticate_ShouldReturnFalseAndRecordFailedLogin_WhenPasswordIsInvalid()
    {
        // Arrange
        var user = CreateUser();
        var loginTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        var result = UserAuthenticationService.TryAuthenticate(user, false, loginTime);

        // Assert
        result.Should().BeFalse();
        user.FailedLoginAttempts.Should().Be(1);
    }

    [Fact]
    public void TryAuthenticate_ShouldThrowAccountInactive_WhenUserIsInactive()
    {
        // Arrange
        var user = CreateUser();
        var loginTime = _fakeTime.GetUtcNow().UtcDateTime;

        user.Deactivate();

        // Act
        var act = () => UserAuthenticationService.TryAuthenticate(user, true, loginTime);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.User.AccountInactive);
    }

    [Fact]
    public void TryAuthenticate_ShouldThrowAccountLockedOut_WhenUserIsLockedOut()
    {
        // Arrange
        var user = CreateUser();
        var lockTime = _fakeTime.GetUtcNow().UtcDateTime;

        for (var i = 0; i < User.MaxFailedLoginAttempts; i++)
            user.RecordFailedLogin(lockTime);

        _fakeTime.Advance(TimeSpan.FromMinutes(5));

        // Act
        var act = () =>
            UserAuthenticationService.TryAuthenticate(
                user,
                true,
                _fakeTime.GetUtcNow().UtcDateTime
            );

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.User.AccountLockedOut);
    }

    [Fact]
    public void TryAuthenticate_ShouldResetFailedAttempts_WhenValidLoginAfterFailures()
    {
        // Arrange
        var user = CreateUser();
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;

        user.RecordFailedLogin(referenceTime);
        user.RecordFailedLogin(referenceTime);
        user.RecordFailedLogin(referenceTime);
        user.RecordFailedLogin(referenceTime);

        // Act
        var result = UserAuthenticationService.TryAuthenticate(user, true, referenceTime);

        // Assert
        result.Should().BeTrue();
        user.FailedLoginAttempts.Should().Be(0);
        user.LastLoginAt.Should().Be(referenceTime);
    }

    private static User CreateUser() =>
        User.Create(
            EmailAddress.Create("test@clinic.com"),
            "hashedpassword123",
            PhoneNumber.Create("555-1234"),
            UserRole.Patient
        );
}
