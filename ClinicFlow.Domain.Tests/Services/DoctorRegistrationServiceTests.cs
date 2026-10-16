using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.Services;
using ClinicFlow.Domain.Services.Args.Registration;
using ClinicFlow.Domain.Services.Contexts;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Tests.Services;

public class DoctorRegistrationServiceTests
{
    [Fact]
    public void Register_ShouldCreateDoctor_WhenExistingDoctorIsNull()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var args = CreateValidArgs();
        var context = new DoctorRegistrationContext { UserId = userId };

        // Act
        var doctor = DoctorRegistrationService.Register(args, context);

        // Assert
        doctor.Should().NotBeNull();
        doctor.UserId.Should().Be(userId);
        doctor.FullName.Should().Be(args.FullName);
        doctor.LicenseNumber.Should().Be(args.LicenseNumber);
        doctor.MedicalSpecialtyId.Should().Be(args.MedicalSpecialtyId);
        doctor.Biography.Should().Be(args.Biography);
        doctor.Room.Should().Be(args.Room);
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenArgsIsNull()
    {
        // Arrange & Act
        var act = () => DoctorRegistrationService.Register(null!, CreateValidContext());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenFullNameIsNull()
    {
        // Arrange
        var args = CreateValidArgs() with
        {
            FullName = null!,
        };

        // Act
        var act = () => DoctorRegistrationService.Register(args, CreateValidContext());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenLicenseNumberIsNull()
    {
        // Arrange
        var args = CreateValidArgs() with
        {
            LicenseNumber = null!,
        };

        // Act
        var act = () => DoctorRegistrationService.Register(args, CreateValidContext());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenRoomIsNull()
    {
        // Arrange
        var args = CreateValidArgs() with
        {
            Room = null!,
        };

        // Act
        var act = () => DoctorRegistrationService.Register(args, CreateValidContext());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Register_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        // Arrange
        var args = CreateValidArgs();

        // Act
        var act = () => DoctorRegistrationService.Register(args, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Register_ShouldThrowDomainValidationException_WhenExistingDoctorIsNotNull()
    {
        // Arrange
        var existingDoctor = Doctor.Create(
            Guid.CreateVersion7(),
            PersonName.Create("Test Doctor"),
            MedicalLicenseNumber.Create("12345"),
            Guid.CreateVersion7(),
            "Old Bio",
            ConsultationRoom.Create(1, "Old Room", 1)
        );

        var args = CreateValidArgs();
        var context = CreateValidContext() with { ExistingDoctor = existingDoctor };

        // Act
        var act = () => DoctorRegistrationService.Register(args, context);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Doctor.InactiveProfileExists);
    }

    private static DoctorRegistrationArgs CreateValidArgs() =>
        new()
        {
            FullName = PersonName.Create("Test Doctor"),
            LicenseNumber = MedicalLicenseNumber.Create("12345"),
            MedicalSpecialtyId = Guid.CreateVersion7(),
            Biography = "Cardiologist",
            Room = ConsultationRoom.Create(1, "Room A", 1),
        };

    private static DoctorRegistrationContext CreateValidContext() =>
        new() { UserId = Guid.CreateVersion7(), ExistingDoctor = null };
}
