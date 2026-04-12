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
        var command = new UpdateDoctorProfileCommand(Guid.NewGuid(), "Updated biography", 205);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenBiographyIsEmpty()
    {
        // Arrange
        var command = new UpdateDoctorProfileCommand(Guid.NewGuid(), "", 101);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDoctorIdIsEmpty()
    {
        // Arrange
        var command = new UpdateDoctorProfileCommand(Guid.Empty, "Biography", 101);

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
        var command = new UpdateDoctorProfileCommand(Guid.NewGuid(), "Biography", roomNumber);

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConsultationRoomNumber);
    }
}
