using ClinicFlow.Application.Doctors.Commands.ReactivateDoctorProfile;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.ValueObjects;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Doctors.Commands.ReactivateDoctorProfile;

public class ReactivateDoctorProfileCommandValidatorTests
{
    private readonly ReactivateDoctorProfileCommandValidator _sut = new();

    [Fact]
    public void Validate_ShouldPass_WhenValidCommand()
    {
        // Arrange
        var command = new ReactivateDoctorProfileCommand(
            Guid.NewGuid(),
            "Biography",
            5,
            "Pediatrics C",
            2
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldFail_WhenDoctorIdIsEmpty()
    {
        // Arrange
        var command = new ReactivateDoctorProfileCommand(Guid.Empty, "Biography", 1, "Room", 1);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DoctorId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Theory]
    [InlineData(ConsultationRoom.MinimumNumber - 1)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_ShouldFail_WhenConsultationRoomNumberIsZero(int roomNumber)
    {
        // Arrange
        var command = new ReactivateDoctorProfileCommand(
            Guid.NewGuid(),
            "Bio",
            roomNumber,
            "Room",
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
    [InlineData(ConsultationRoom.MaximumNumber + 1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_ShouldFail_WhenConsultationRoomNumberExceedsMaximum(int roomNumber)
    {
        // Arrange
        var command = new ReactivateDoctorProfileCommand(
            Guid.NewGuid(),
            "Bio",
            roomNumber,
            "Room",
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
    public void Validate_ShouldFail_WhenConsultationRoomNameIsEmpty(string? roomName)
    {
        // Arrange
        var command = new ReactivateDoctorProfileCommand(
            Guid.NewGuid(),
            "Biography",
            1,
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

    [Fact]
    public void Validate_ShouldFail_WhenConsultationRoomFloorIsBelowMinimum()
    {
        // Arrange
        var command = new ReactivateDoctorProfileCommand(
            Guid.NewGuid(),
            "Bio",
            1,
            "Room",
            ConsultationRoom.MinimumFloor - 1
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ConsultationRoomFloor)
            .WithErrorMessage(DomainErrors.Validation.ValueMustBePositive);
    }

    [Fact]
    public void Validate_ShouldFail_WhenConsultationRoomFloorExceedsMaximum()
    {
        // Arrange
        var command = new ReactivateDoctorProfileCommand(
            Guid.NewGuid(),
            "Bio",
            1,
            "Room",
            ConsultationRoom.MaximumFloor + 1
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ConsultationRoomFloor)
            .WithErrorMessage(DomainErrors.Validation.ValueExceedsMaximum);
    }
}
