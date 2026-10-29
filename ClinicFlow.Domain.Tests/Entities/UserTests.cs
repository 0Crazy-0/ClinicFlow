using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Events.Users;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Domain.Tests.Entities;

public class UserTests
{
    private readonly FakeTimeProvider _fakeTime = new();

    [Fact]
    public void Create_ShouldCreateUser_WhenValidParameters()
    {
        // Arrange
        var email = EmailAddress.Create("test@clinic.com");
        var passwordHash = "hashedpassword123";
        var phone = PhoneNumber.Create("555-1234");
        var role = UserRole.Doctor;

        // Act
        var user = User.Create(email, passwordHash, phone, role);

        // Assert
        user.Should().NotBeNull();
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(passwordHash);
        user.Phone.Should().Be(phone);
        user.Role.Should().Be(role);
        user.IsActive.Should().BeTrue();
        user.LastLoginAt.Should().BeNull();
        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_ShouldThrowException_WhenPasswordHashIsEmpty(string? invalidHash)
    {
        // Arrange & Act
        var act = () =>
            User.Create(
                EmailAddress.Create("test@clinic.com"),
                invalidHash!,
                PhoneNumber.Create("555-1234"),
                UserRole.Doctor
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void CreatePending_ShouldCreatePatientUser_WithoutPassword()
    {
        // Arrange
        var email = EmailAddress.Create("p@x.com");
        var phone = PhoneNumber.Create("555-1234");

        // Act
        var user = User.CreatePending(email, phone);

        // Assert
        user.Email.Should().Be(email);
        user.Role.Should().Be(UserRole.Patient);
        user.PasswordHash.Should().BeNull();
        user.ActivatedAt.Should().BeNull();
        user.IsActive.Should().BeTrue();
        user.Phone.Should().Be(phone);
    }

    [Fact]
    public void Activate_ShouldSetPasswordHashAndActivatedAt_AndRaiseEvent()
    {
        // Arrange
        var user = User.CreatePending(
            EmailAddress.Create("p@x.com"),
            PhoneNumber.Create("555-1234")
        );

        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        user.Activate("hash", referenceTime);

        // Assert
        user.PasswordHash.Should().Be("hash");
        user.ActivatedAt.Should().Be(referenceTime);

        var domainEvent = user
            .DomainEvents.OfType<PatientAccountActivatedEvent>()
            .Should()
            .ContainSingle()
            .Which;

        domainEvent.UserId.Should().Be(user.Id);
        domainEvent.ActivatedAt.Should().Be(referenceTime);
    }

    [Fact]
    public void Activate_ShouldThrowBusinessRuleValidationException_WhenAlreadyActivated()
    {
        // Arrange
        var user = User.CreatePending(
            EmailAddress.Create("p@x.com"),
            PhoneNumber.Create("555-1234")
        );

        user.Activate("hash", _fakeTime.GetUtcNow().UtcDateTime);

        // Act
        var act = () => user.Activate("anotherhash67", _fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.User.AlreadyActivated);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Activate_ShouldThrowDomainValidationException_WhenPasswordHashIsNullOrWhiteSpace(
        string? invalidHash
    )
    {
        // Arrange
        var user = User.CreatePending(
            EmailAddress.Create("p@x.com"),
            PhoneNumber.Create("555-1234")
        );

        // Act
        var act = () => user.Activate(invalidHash!, _fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Activate_ShouldThrowDomainValidationException_WhenReferenceTimeIsDefault()
    {
        // Arrange
        var user = User.CreatePending(
            EmailAddress.Create("p@x.com"),
            PhoneNumber.Create("555-1234")
        );

        // Act
        var act = () => user.Activate("hash", default);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void RecordLogin_ShouldUpdateLastLoginAt_WhenAccountIsActive()
    {
        // Arrange
        var user = CreateUser();
        var loginTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        user.RecordLogin(loginTime);

        // Assert
        user.LastLoginAt.Should().Be(loginTime);
        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
    }

    [Fact]
    public void RecordLogin_ShouldResetFailedAttempts_WhenPreviousFailedAttemptsExist()
    {
        // Arrange
        var user = CreateUser();
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;

        user.RecordFailedLogin(referenceTime);
        user.RecordFailedLogin(referenceTime);

        // Act
        user.RecordLogin(referenceTime);

        // Assert
        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
        user.LastLoginAt.Should().Be(referenceTime);
    }

    [Fact]
    public void RecordLogin_ShouldThrowException_WhenAccountIsInactive()
    {
        // Arrange
        var user = CreateUser();

        user.Deactivate();

        // Act
        var act = () => user.RecordLogin(_fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.User.AccountInactive);
    }

    [Fact]
    public void RecordLogin_ShouldThrowException_WhenAccountIsLockedOut()
    {
        // Arrange
        var user = CreateUser();
        var lockTime = _fakeTime.GetUtcNow().UtcDateTime;

        LockOutUser(user, lockTime);

        _fakeTime.Advance(TimeSpan.FromMinutes(5));

        // Act
        var act = () => user.RecordLogin(_fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.User.AccountLockedOut);
    }

    [Fact]
    public void RecordLogin_ShouldResetLockoutState_WhenLockoutEndEqualsLoginTime()
    {
        // Arrange
        var user = CreateUser();
        var lockTime = _fakeTime.GetUtcNow().UtcDateTime;

        LockOutUser(user, lockTime);

        var loginTime = user.LockoutEnd!.Value;

        // Act
        user.RecordLogin(loginTime);

        // Assert
        user.LastLoginAt.Should().Be(loginTime);
        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
    }

    [Fact]
    public void RecordFailedLogin_ShouldIncrementFailedAttempts()
    {
        // Arrange
        var user = CreateUser();
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;

        // Act
        user.RecordFailedLogin(referenceTime);

        // Assert
        user.FailedLoginAttempts.Should().Be(1);
        user.LockoutEnd.Should().BeNull();
    }

    [Fact]
    public void RecordFailedLogin_ShouldLockAccount_WhenMaxAttemptsReached()
    {
        // Arrange
        var user = CreateUser();
        var referenceTime = _fakeTime.GetUtcNow().UtcDateTime;

        for (var i = 0; i < User.MaxFailedLoginAttempts - 1; i++)
            user.RecordFailedLogin(referenceTime);

        // Act
        user.RecordFailedLogin(referenceTime);

        // Assert
        user.FailedLoginAttempts.Should().Be(User.MaxFailedLoginAttempts);
        user.LockoutEnd.Should().Be(referenceTime.AddMinutes(15));
    }

    [Fact]
    public void RecordFailedLogin_ShouldThrowException_WhenReferenceTimeIsDefault()
    {
        // Arrange
        var user = CreateUser();

        // Act
        var act = () => user.RecordFailedLogin(default);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);

        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
    }

    [Fact]
    public void RecordFailedLogin_ShouldThrowException_WhenAccountIsInactive()
    {
        // Arrange
        var user = CreateUser();

        user.Deactivate();

        // Act
        var act = () => user.RecordFailedLogin(_fakeTime.GetUtcNow().UtcDateTime);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.User.AccountInactive);
    }

    [Fact]
    public void ChangePassword_ShouldUpdateHash_WhenValidHash()
    {
        // Arrange
        var user = CreateUser();
        var newHash = "newhashedpassword456";

        // Act
        user.ChangePassword(newHash);

        // Assert
        user.PasswordHash.Should().Be(newHash);
        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
    }

    [Fact]
    public void ChangePassword_ShouldResetLockoutState_WhenUserWasLockedOut()
    {
        // Arrange
        var user = CreateUser();
        var lockTime = _fakeTime.GetUtcNow().UtcDateTime;
        LockOutUser(user, lockTime);

        // Act
        user.ChangePassword("newhashedpassword456");

        // Assert
        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ChangePassword_ShouldThrowException_WhenHashIsEmpty(string? invalidHash)
    {
        // Arrange
        var user = CreateUser();

        // Act
        var act = () => user.ChangePassword(invalidHash!);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = CreateUser();

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldThrowException_WhenAlreadyInactive()
    {
        // Arrange
        var user = CreateUser();

        user.Deactivate();

        // Act & Assert
        user.Invoking(u => u.Deactivate())
            .Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.User.AlreadyInactive);
    }

    [Fact]
    public void Reactivate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var user = CreateUser();

        user.Deactivate();

        // Act
        user.Reactivate();

        // Assert
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Reactivate_ShouldResetLockoutState()
    {
        // Arrange
        var user = CreateUser();
        var lockTime = _fakeTime.GetUtcNow().UtcDateTime;

        LockOutUser(user, lockTime);

        user.Deactivate();

        // Act
        user.Reactivate();

        // Assert
        user.IsActive.Should().BeTrue();
        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
    }

    [Fact]
    public void Reactivate_ShouldThrowException_WhenAlreadyActive()
    {
        // Arrange
        var user = CreateUser();

        // Act & Assert
        user.Invoking(u => u.Reactivate())
            .Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.User.AlreadyActive);
    }

    private static User CreateUser() =>
        User.Create(
            EmailAddress.Create("test@clinic.com"),
            "hashedpassword123",
            PhoneNumber.Create("555-1234"),
            UserRole.Doctor
        );

    private static void LockOutUser(User user, DateTime referenceTime)
    {
        for (var i = 0; i < User.MaxFailedLoginAttempts; i++)
            user.RecordFailedLogin(referenceTime);
    }
}
