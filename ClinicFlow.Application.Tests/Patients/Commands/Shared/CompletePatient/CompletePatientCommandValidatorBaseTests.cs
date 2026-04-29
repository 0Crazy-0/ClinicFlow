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
    public void Validate_ShouldHaveError_WhenUserIdIsEmpty()
    {
        // Arrange
        var command = new DummyCompletePatientCommand(
            Guid.Empty,
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
        result
            .ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenFirstNameIsEmptyOrTooShort()
    {
        // Arrange
        var command = new DummyCompletePatientCommand(
            Guid.NewGuid(),
            "J",
            "Doe",
            DateTime.UtcNow.AddYears(-30),
            "O+",
            "Mom",
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenLastNameIsEmptyOrTooShort()
    {
        // Arrange
        var command = new DummyCompletePatientCommand(
            Guid.NewGuid(),
            "John",
            "",
            DateTime.UtcNow.AddYears(-30),
            "O+",
            "Mom",
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDateOfBirthIsInTheFuture()
    {
        // Arrange
        var command = new DummyCompletePatientCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateTime.UtcNow.AddDays(1),
            "O+",
            "Mom",
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage(DomainErrors.Validation.ValueCannotBeInFuture);
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
