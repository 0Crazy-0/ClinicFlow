using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;
using FluentAssertions;

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
            LicenseNumber = MedicalLicenseNumber.Create("12345"),
            MedicalSpecialtyId = Guid.NewGuid(),
            Biography = "Cardiologist",
            ConsultationRoom = ConsultationRoom.Create(1, "Room A", 1),
        };

        var context = new DoctorRegistrationContext { ExistingDoctor = null };

        // Act
        var doctor = DoctorRegistrationService.Register(args, context);

        // Assert
        doctor.Should().NotBeNull();
        doctor.UserId.Should().Be(args.UserId);
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
            MedicalLicenseNumber.Create("12345"),
            Guid.NewGuid(),
            "Old Bio",
            ConsultationRoom.Create(1, "Old Room", 1)
        );

        var args = new DoctorRegistrationArgs
        {
            UserId = Guid.NewGuid(),
            LicenseNumber = MedicalLicenseNumber.Create("12345"),
            MedicalSpecialtyId = Guid.NewGuid(),
            Biography = "Cardiologist",
            ConsultationRoom = ConsultationRoom.Create(1, "Room A", 1),
        };

        var context = new DoctorRegistrationContext { ExistingDoctor = existingDoctor };

        // Act
        var act = () => DoctorRegistrationService.Register(args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Doctor.InactiveProfileExists);
    }
}
