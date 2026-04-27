using ClinicFlow.Application.Doctors.Commands.UpdateDoctorProfile;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Doctors.Commands.UpdateDoctorProfile;

public class UpdateDoctorProfileCommandValidatorTests
{
    private readonly UpdateDoctorProfileCommandValidator _sut;

    public UpdateDoctorProfileCommandValidatorTests()
    {
        _sut = new UpdateDoctorProfileCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new UpdateDoctorProfileCommand(
            Guid.NewGuid(),
            "Updated biography",
            20,
            "Dermatology B",
            5
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenBiographyIsEmpty()
    {
        // Arrange
        var command = new UpdateDoctorProfileCommand(Guid.NewGuid(), "", 10, "Room A", 1);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDoctorIdIsEmpty()
    {
        // Arrange
        var command = new UpdateDoctorProfileCommand(Guid.Empty, "Biography", 10, "Room A", 1);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DoctorId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_ShouldHaveError_WhenConsultationRoomNumberIsZeroOrNegative(int roomNumber)
    {
        // Arrange
        var command = new UpdateDoctorProfileCommand(
            Guid.NewGuid(),
            "Biography",
            roomNumber,
            "Room A",
            1
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConsultationRoomNumber);
    }

    [Theory]
    [InlineData(36)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_ShouldHaveError_WhenConsultationRoomNumberExceedsMaximum(int roomNumber)
    {
        // Arrange
        var command = new UpdateDoctorProfileCommand(
            Guid.NewGuid(),
            "Biography",
            roomNumber,
            "Room A",
            1
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConsultationRoomNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenConsultationRoomNameIsEmpty(string? roomName)
    {
        // Arrange
        var command = new UpdateDoctorProfileCommand(Guid.NewGuid(), "Biography", 10, roomName!, 1);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConsultationRoomName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_ShouldHaveError_WhenConsultationRoomFloorIsZeroOrNegative(int floor)
    {
        // Arrange
        var command = new UpdateDoctorProfileCommand(
            Guid.NewGuid(),
            "Biography",
            10,
            "Room A",
            floor
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConsultationRoomFloor);
    }

    [Theory]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(100)]
    public void Validate_ShouldHaveError_WhenConsultationRoomFloorExceedsMaximum(int floor)
    {
        // Arrange
        var command = new UpdateDoctorProfileCommand(
            Guid.NewGuid(),
            "Biography",
            10,
            "Room A",
            floor
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConsultationRoomFloor);
    }
}
