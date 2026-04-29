using ClinicFlow.Application.Patients.Commands.Shared.CompletePatient;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Patients.Commands.Shared.CompletePatient;

public record DummyCompletePatientCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string BloodType,
    string EmergencyContactName,
    string EmergencyContactPhone
) : ICompletePatientCommand;

public class DummyCompletePatientCommandValidator
    : CompletePatientCommandValidatorBase<DummyCompletePatientCommand> { }

public class CompletePatientCommandValidatorBaseTests
{
    private readonly DummyCompletePatientCommandValidator _sut;

    public CompletePatientCommandValidatorBaseTests()
    {
        _sut = new DummyCompletePatientCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new DummyCompletePatientCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-30),
            "O+",
            "Mom",
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenBloodTypeIsEmpty()
    {
        // Arrange
        var command = new DummyCompletePatientCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-30),
            "",
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
        var command = new DummyCompletePatientCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-30),
            "O+",
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
        var command = new DummyCompletePatientCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-30),
            "O+",
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
