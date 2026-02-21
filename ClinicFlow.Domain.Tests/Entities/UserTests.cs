using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;
using ClinicFlow.Domain.Exceptions.Base;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Entities;

public class UserTests
{
    // Create
    [Fact]
    public void Create_ShouldCreateUser_WhenValidParameters()
    {
        // Arrange
        var email = EmailAddress.Create("test@clinic.com");
        var passwordHash = "hashedpassword123";
        var fullName = PersonName.Create("John Doe");
        var phone = PhoneNumber.Create("555-1234");
        var role = UserRole.Doctor;

        // Act
        var user = User.Create(email, passwordHash, fullName, phone, role);

        // Assert
        user.Should().NotBeNull();
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(passwordHash);
        user.FullName.Should().Be(fullName);
        user.PhoneNumber.Should().Be(phone);
        user.Role.Should().Be(role);
        user.IsActive.Should().BeTrue();
        user.LastLoginAt.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldCreateUser_WithOptionalIds()
    {
        // Arrange
        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();

        // Act
        var user = User.Create(EmailAddress.Create("doc@clinic.com"), "hash", PersonName.Create("Dr. García"), PhoneNumber.Create("555-0000"), UserRole.Doctor, doctorId, patientId);

        // Assert
        user.DoctorId.Should().Be(doctorId);
        user.PatientId.Should().Be(patientId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_ShouldThrowException_WhenPasswordHashIsEmpty(string? invalidHash)
    {
        // Arrange & Act
        var act = () => User.Create(EmailAddress.Create("test@clinic.com"), invalidHash!, PersonName.Create("John Doe"), PhoneNumber.Create("555-1234"), UserRole.Doctor);

        // Assert
        act.Should().Throw<DomainValidationException>().WithMessage("Password hash cannot be empty.");
    }

    [Fact]
    public void Create_ShouldThrowException_WhenDoctorIdIsEmpty()
    {
        // Arrange & Act
        var act = () => User.Create(EmailAddress.Create("test@clinic.com"), "hash", PersonName.Create("John Doe"), PhoneNumber.Create("555-1234"), UserRole.Doctor, Guid.Empty);

        // Assert
        act.Should().Throw<DomainValidationException>().WithMessage("Doctor ID cannot be empty.");
    }

    [Fact]
    public void Create_ShouldThrowException_WhenPatientIdIsEmpty()
    {
        // Arrange & Act
        var act = () => User.Create(EmailAddress.Create("test@clinic.com"), "hash", PersonName.Create("John Doe"), PhoneNumber.Create("555-1234"), UserRole.Patient,
            patientId: Guid.Empty);

        // Assert
        act.Should().Throw<DomainValidationException>().WithMessage("Patient ID cannot be empty.");
    }
}
