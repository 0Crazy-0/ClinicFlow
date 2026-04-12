using ClinicFlow.Application.Doctors.Commands.CreateDoctorProfile;
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
            101
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
            101
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
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
            101
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LicenseNumber);
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
            101
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LicenseNumber);
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
            101
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MedicalSpecialtyId);
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
            roomNumber
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConsultationRoomNumber);
    }
}
