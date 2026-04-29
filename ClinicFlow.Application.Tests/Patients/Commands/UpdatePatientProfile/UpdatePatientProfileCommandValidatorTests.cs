using ClinicFlow.Application.Patients.Commands.UpdatePatientProfile;
using ClinicFlow.Domain.Common;
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
            Guid.NewGuid(),
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

    [Fact]
    public void Validate_ShouldHaveError_WhenBloodTypeIsEmpty()
    {
        // Arrange
        var command = new UpdatePatientProfileCommand(
            Guid.NewGuid(),
            "",
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

    [Fact]
    public void Validate_ShouldHaveError_WhenEmergencyContactNameIsEmpty()
    {
        // Arrange
        var command = new UpdatePatientProfileCommand(
            Guid.NewGuid(),
            "O+",
            "None",
            "None",
            "",
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
    public void Validate_ShouldHaveError_WhenEmergencyContactPhoneIsEmpty()
    {
        // Arrange
        var command = new UpdatePatientProfileCommand(
            Guid.NewGuid(),
            "O+",
            "None",
            "None",
            "Mom",
            ""
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EmergencyContactPhone)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }
}
