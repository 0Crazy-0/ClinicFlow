using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;
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

    [Fact]
    public void Create_ShouldHaveNullOptionalIds_WhenNotProvided()
    {
        // Arrange & Act
        var user = User.Create(EmailAddress.Create("admin@clinic.com"), "hash", PersonName.Create("Admin"), PhoneNumber.Create("555-9999"), UserRole.Admin);

        // Assert
        user.DoctorId.Should().BeNull();
        user.PatientId.Should().BeNull();
    }
}
