using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

namespace ClinicFlow.Domain.Tests.Entities;

public class DoctorTests
{
    [Fact]
    public void Create_ShouldCreateDoctor_WhenValidParameters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var licenseNumber = MedicalLicenseNumber.Create("12345");
        var specialtyId = Guid.NewGuid();
        var biography = "Cardiologist with 10 years of experience";
        var room = ConsultationRoom.Create(1, "Cardiology A", 3);

        // Act
        var doctor = Doctor.Create(userId, licenseNumber, specialtyId, biography, room);

        // Assert
        doctor.Should().NotBeNull();
        doctor.UserId.Should().Be(userId);
        doctor.LicenseNumber.Should().Be(licenseNumber);
        doctor.MedicalSpecialtyId.Should().Be(specialtyId);
        doctor.Biography.Should().Be(biography);
        doctor.ConsultationRoom.Should().Be(room);
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
                ConsultationRoom.Create(1, "Room A", 1)
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
                ConsultationRoom.Create(1, "Room A", 1)
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void UpdateProfile_ShouldUpdateBiographyAndConsultationRoom_WhenValidParameters()
    {
        // Arrange
        var doctor = Doctor.Create(
            Guid.NewGuid(),
            MedicalLicenseNumber.Create("12345"),
            Guid.NewGuid(),
            "Original biography",
            ConsultationRoom.Create(1, "Room A", 1)
        );

        var newBiography = "Updated biography with new certifications";
        var newRoom = ConsultationRoom.Create(2, "Dermatology B", 5);

        // Act
        doctor.UpdateProfile(newBiography, newRoom);

        // Assert
        doctor.Biography.Should().Be(newBiography);
        doctor.ConsultationRoom.Should().Be(newRoom);
    }
}
