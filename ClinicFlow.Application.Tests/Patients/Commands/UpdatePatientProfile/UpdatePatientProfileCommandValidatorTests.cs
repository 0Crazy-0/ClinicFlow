using ClinicFlow.Application.Patients.Commands.UpdatePatientProfile;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.ValueObjects;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Patients.Commands.UpdatePatientProfile;

public class UpdatePatientProfileCommandValidatorTests
{
    private readonly UpdatePatientProfileCommandValidator _sut;

    public UpdatePatientProfileCommandValidatorTests()
    {
        _sut = new UpdatePatientProfileCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            "O+",
            "None",
            "None",
            "Mom",
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPatientIdIsEmpty()
    {
        // Arrange
        var command = new UpdatePatientProfileCommand(
            Guid.Empty,
            "O+",
            "None",
            "None",
            "Mom",
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PatientId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenBloodTypeIsEmpty(string? bloodType)
    {
        // Arrange
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            bloodType!,
            "None",
            "None",
            "Mom",
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.BloodType)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenEmergencyContactNameIsEmpty(
        string? emergencyContactName
    )
    {
        // Arrange
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            "O+",
            "None",
            "None",
            emergencyContactName!,
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EmergencyContactName)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEmergencyContactNameIsTooShort()
    {
        // Arrange
        var emergencyContactName = new string('A', PersonName.MinimumLength - 1);
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            "O+",
            "None",
            "None",
            emergencyContactName,
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EmergencyContactName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEmergencyContactNameIsTooLong()
    {
        // Arrange
        var emergencyContactName = new string('A', PersonName.MaximumLength + 1);
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            "O+",
            "None",
            "None",
            emergencyContactName,
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EmergencyContactName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenEmergencyContactPhoneIsEmpty(
        string? emergencyContactPhone
    )
    {
        // Arrange
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            "O+",
            "None",
            "None",
            "Mom",
            emergencyContactPhone!
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EmergencyContactPhone)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEmergencyContactPhoneIsTooShort()
    {
        // Arrange
        var emergencyContactPhone = new string('1', PhoneNumber.MinimumLength - 1);
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            "O+",
            "None",
            "None",
            "Mom",
            emergencyContactPhone
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EmergencyContactPhone)
            .WithErrorMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEmergencyContactPhoneIsTooLong()
    {
        // Arrange
        var emergencyContactPhone = "+" + new string('1', PhoneNumber.MaximumLength);
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            "O+",
            "None",
            "None",
            "Mom",
            emergencyContactPhone
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EmergencyContactPhone)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }
}
