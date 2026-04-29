using ClinicFlow.Application.Doctors.Commands.CreateDoctorProfile;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Doctors.Commands.CreateDoctorProfile;

public class CreateDoctorProfileCommandValidatorTests
{
    private readonly CreateDoctorProfileCommandValidator _sut;

    public CreateDoctorProfileCommandValidatorTests()
    {
        _sut = new CreateDoctorProfileCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.NewGuid(),
            "12345",
            Guid.NewGuid(),
            "Cardiologist with 10 years of experience",
            10,
            "Cardiology A",
            3
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenUserIdIsEmpty()
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.Empty,
            "12345",
            Guid.NewGuid(),
            "Biography",
            10,
            "Room A",
            1
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenLicenseNumberIsEmpty()
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.NewGuid(),
            "",
            Guid.NewGuid(),
            "Biography",
            10,
            "Room A",
            1
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.LicenseNumber)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenLicenseNumberIsTooShort()
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.NewGuid(),
            "123",
            Guid.NewGuid(),
            "Biography",
            10,
            "Room A",
            1
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.LicenseNumber)
            .WithErrorMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenMedicalSpecialtyIdIsEmpty()
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.NewGuid(),
            "12345",
            Guid.Empty,
            "Biography",
            10,
            "Room A",
            1
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MedicalSpecialtyId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_ShouldHaveError_WhenConsultationRoomNumberIsZeroOrNegative(int roomNumber)
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.NewGuid(),
            "12345",
            Guid.NewGuid(),
            "Biography",
            roomNumber,
            "Room A",
            1
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ConsultationRoomNumber)
            .WithErrorMessage(DomainErrors.Validation.ValueMustBePositive);
    }

    [Theory]
    [InlineData(36)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_ShouldHaveError_WhenConsultationRoomNumberExceedsMaximum(int roomNumber)
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.NewGuid(),
            "12345",
            Guid.NewGuid(),
            "Biography",
            roomNumber,
            "Room A",
            1
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ConsultationRoomNumber)
            .WithErrorMessage(DomainErrors.Validation.ValueExceedsMaximum);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenConsultationRoomNameIsEmpty(string? roomName)
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.NewGuid(),
            "12345",
            Guid.NewGuid(),
            "Biography",
            10,
            roomName!,
            1
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ConsultationRoomName)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_ShouldHaveError_WhenConsultationRoomFloorIsZeroOrNegative(int floor)
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.NewGuid(),
            "12345",
            Guid.NewGuid(),
            "Biography",
            10,
            "Room A",
            floor
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ConsultationRoomFloor)
            .WithErrorMessage(DomainErrors.Validation.ValueMustBePositive);
    }

    [Theory]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(100)]
    public void Validate_ShouldHaveError_WhenConsultationRoomFloorExceedsMaximum(int floor)
    {
        // Arrange
        var command = new CreateDoctorProfileCommand(
            Guid.NewGuid(),
            "12345",
            Guid.NewGuid(),
            "Biography",
            10,
            "Room A",
            floor
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ConsultationRoomFloor)
            .WithErrorMessage(DomainErrors.Validation.ValueExceedsMaximum);
    }
}
