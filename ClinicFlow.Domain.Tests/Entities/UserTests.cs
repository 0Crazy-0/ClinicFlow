using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Entities;

public class UserTests
{
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
        user.PhoneNumber.Should().Be(phone);
        user.Role.Should().Be(role);
        user.IsActive.Should().BeTrue();
        user.LastLoginAt.Should().BeNull();
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
}
