using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Tests.Services;

public class DoctorRegistrationServiceTests
{
    [Fact]
    public void Register_ShouldCreateDoctor_WhenExistingDoctorIsNull()
    {
        // Arrange
        var args = new DoctorRegistrationArgs
        {
            UserId = Guid.NewGuid(),
            FullName = PersonName.Create("Test Doctor"),
            LicenseNumber = MedicalLicenseNumber.Create("12345"),
            MedicalSpecialtyId = Guid.NewGuid(),
            Biography = "Cardiologist",
            ConsultationRoom = ConsultationRoom.Create(1, "Room A", 1),
        };

        // Act
        var doctor = DoctorRegistrationService.Register(args, null);

        // Assert
        doctor.Should().NotBeNull();
        doctor.UserId.Should().Be(args.UserId);
        doctor.FullName.Should().Be(args.FullName);
        doctor.LicenseNumber.Should().Be(args.LicenseNumber);
        doctor.MedicalSpecialtyId.Should().Be(args.MedicalSpecialtyId);
        doctor.Biography.Should().Be(args.Biography);
        doctor.ConsultationRoom.Should().Be(args.ConsultationRoom);
    }

    [Fact]
    public void Register_ShouldThrowDomainValidationException_WhenExistingDoctorIsNotNull()
    {
        // Arrange
        var existingDoctor = Doctor.Create(
            Guid.NewGuid(),
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("12345"),
            Guid.NewGuid(),
            "Old Bio",
            ConsultationRoom.Create(1, "Old Room", 1)
        );

        var args = new DoctorRegistrationArgs
        {
            UserId = Guid.NewGuid(),
            FullName = PersonName.Create("Test Doctor"),
            LicenseNumber = MedicalLicenseNumber.Create("12345"),
            MedicalSpecialtyId = Guid.NewGuid(),
            Biography = "Cardiologist",
            ConsultationRoom = ConsultationRoom.Create(1, "Room A", 1),
        };

        // Act
        var act = () => DoctorRegistrationService.Register(args, existingDoctor);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Doctor.InactiveProfileExists);
    }
}
