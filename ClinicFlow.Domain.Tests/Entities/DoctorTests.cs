using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Entities;
using ClinicFlow.Domain.Events.Doctors;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Tests.Entities;

public class DoctorTests
{
    [Fact]
    public void Create_ShouldCreateDoctor_WhenValidParameters()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var fullName = PersonName.Create("Doctor", "Test", "sixseven");
        var licenseNumber = MedicalLicenseNumber.Create("12345");
        var specialtyId = Guid.CreateVersion7();
        var biography = "Cardiologist with 10 years of experience";
        var room = ConsultationRoom.Create(1, "Cardiology A", 3);

        // Act
        var doctor = Doctor.Create(userId, fullName, licenseNumber, specialtyId, biography, room);

        // Assert
        doctor.Should().NotBeNull();
        doctor.UserId.Should().Be(userId);
        doctor.FullName.Should().Be(fullName);
        doctor.LicenseNumber.Should().Be(licenseNumber);
        doctor.MedicalSpecialtyId.Should().Be(specialtyId);
        doctor.Biography.Should().Be(biography);
        doctor.Room.Should().Be(room);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenUserIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            Doctor.Create(
                Guid.Empty,
                PersonName.Create("Test", null, "Doctor"),
                MedicalLicenseNumber.Create("12345"),
                Guid.CreateVersion7(),
                "Biography",
                ConsultationRoom.Create(1, "Room A", 1)
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenFullNameIsNull()
    {
        // Arrange & Act
        var act = () =>
            Doctor.Create(
                Guid.CreateVersion7(),
                null!,
                MedicalLicenseNumber.Create("12345"),
                Guid.CreateVersion7(),
                "Biography",
                ConsultationRoom.Create(1, "Room A", 1)
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenLicenseNumberIsNull()
    {
        // Arrange & Act
        var act = () =>
            Doctor.Create(
                Guid.CreateVersion7(),
                PersonName.Create("Test", null, "Doctor"),
                null!,
                Guid.CreateVersion7(),
                "Biography",
                ConsultationRoom.Create(1, "Room A", 1)
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenMedicalSpecialtyIdIsEmpty()
    {
        // Arrange & Act
        var act = () =>
            Doctor.Create(
                Guid.CreateVersion7(),
                PersonName.Create("Test", null, "Doctor"),
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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenBiographyIsEmpty(string? biography)
    {
        // Arrange & Act
        var act = () =>
            Doctor.Create(
                Guid.CreateVersion7(),
                PersonName.Create("Test", null, "Doctor"),
                MedicalLicenseNumber.Create("12345"),
                Guid.CreateVersion7(),
                biography!,
                ConsultationRoom.Create(1, "Room A", 1)
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenConsultationRoomIsNull()
    {
        // Arrange & Act
        var act = () =>
            Doctor.Create(
                Guid.CreateVersion7(),
                PersonName.Create("Test", null, "Doctor"),
                MedicalLicenseNumber.Create("12345"),
                Guid.CreateVersion7(),
                "Biography",
                null!
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void UpdateProfile_ShouldUpdateBiographyAndConsultationRoom_WhenValidParameters()
    {
        // Arrange
        var doctor = Doctor.Create(
            Guid.CreateVersion7(),
            PersonName.Create("Test", null, "Doctor"),
            MedicalLicenseNumber.Create("12345"),
            Guid.CreateVersion7(),
            "Original biography",
            ConsultationRoom.Create(1, "Room A", 1)
        );

        var newBiography = "Updated biography with new certifications";
        var newRoom = ConsultationRoom.Create(2, "Dermatology B", 5);

        // Act
        doctor.UpdateProfile(newBiography, newRoom);

        // Assert
        doctor.Biography.Should().Be(newBiography);
        doctor.Room.Should().Be(newRoom);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateProfile_ShouldThrowException_WhenBiographyIsEmpty(string? biography)
    {
        // Arrange
        var doctor = CreateDoctor();

        // Act
        var act = () => doctor.UpdateProfile(biography!, ConsultationRoom.Create(1, "Room A", 1));

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void UpdateProfile_ShouldThrowException_WhenConsultationRoomIsNull()
    {
        // Arrange
        var doctor = CreateDoctor();

        // Act
        var act = () => doctor.UpdateProfile("Biography", null!);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.General.RequiredFieldNull);
    }

    [Fact]
    public void Suspend_ShouldMarkAsDeletedAndEmitEvent()
    {
        // Arrange
        var doctor = CreateDoctor();

        // Act
        doctor.Suspend();

        // Assert
        doctor.IsDeleted.Should().BeTrue();
        doctor.DomainEvents.OfType<DoctorSuspendedEvent>().Should().ContainSingle();
    }

    [Fact]
    public void Suspend_ShouldThrowException_WhenAlreadySuspended()
    {
        // Arrange
        var doctor = CreateDoctor();
        doctor.Suspend();

        // Act & Assert
        doctor
            .Invoking(d => d.Suspend())
            .Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Doctor.AlreadySuspended);
    }

    [Fact]
    public void Reactivate_ShouldUndoDeletion()
    {
        // Arrange
        var doctor = CreateDoctor();

        doctor.Suspend();

        // Act
        doctor.Reactivate();

        // Assert
        doctor.IsDeleted.Should().BeFalse();
        doctor.Biography.Should().Be("Biography");
        doctor.Room.Number.Should().Be(1);
        doctor.Room.Name.Should().Be("Room A");
        doctor.Room.Floor.Should().Be(1);
    }

    [Fact]
    public void Reactivate_ShouldThrowException_WhenAlreadyActive()
    {
        // Arrange
        var doctor = CreateDoctor();

        // Act & Assert
        doctor
            .Invoking(d => d.Reactivate())
            .Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Doctor.AlreadyActive);
    }

    private static Doctor CreateDoctor() =>
        Doctor.Create(
            Guid.CreateVersion7(),
            PersonName.Create("Test", null, "Doctor"),
            MedicalLicenseNumber.Create("12345"),
            Guid.CreateVersion7(),
            "Biography",
            ConsultationRoom.Create(1, "Room A", 1)
        );
}
