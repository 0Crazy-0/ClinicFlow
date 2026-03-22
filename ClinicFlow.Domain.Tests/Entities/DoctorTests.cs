using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Entities;

public class DoctorTests
{
    // Create
    [Fact]
    public void Create_ShouldCreateDoctor_WhenValidParameters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var licenseNumber = MedicalLicenseNumber.Create("12345");
        var specialtyId = Guid.NewGuid();
        var biography = "Cardiologist with 10 years of experience";
        var roomNumber = 101;

        // Act
        var doctor = Doctor.Create(userId, licenseNumber, specialtyId, biography, roomNumber);

        // Assert
        doctor.Should().NotBeNull();
        doctor.UserId.Should().Be(userId);
        doctor.LicenseNumber.Should().Be(licenseNumber);
        doctor.MedicalSpecialtyId.Should().Be(specialtyId);
        doctor.Biography.Should().Be(biography);
        doctor.ConsultationRoomNumber.Should().Be(roomNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_ShouldThrowException_WhenConsultationRoomNumberIsZeroOrNegative(
        int roomNumber
    )
    {
        // Arrange & Act
        var act = () =>
            Doctor.Create(
                Guid.NewGuid(),
                MedicalLicenseNumber.Create("12345"),
                Guid.NewGuid(),
                "Biography",
                roomNumber
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueMustBePositive);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenUserIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            Doctor.Create(
                Guid.Empty,
                MedicalLicenseNumber.Create("12345"),
                Guid.NewGuid(),
                "Biography",
                101
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenMedicalSpecialtyIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            Doctor.Create(
                Guid.NewGuid(),
                MedicalLicenseNumber.Create("12345"),
                Guid.Empty,
                "Biography",
                101
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }
}
