using ClinicFlow.Application.Patients.Commands.CreatePatientProfile;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Patients.Commands.CreatePatientProfile;

public class CreatePatientProfileCommandValidatorTests
{
    private readonly CreatePatientProfileCommandValidator _sut;

    public CreatePatientProfileCommandValidatorTests()
    {
        _sut = new CreatePatientProfileCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new CreatePatientProfileCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-30)
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
        var command = new CreatePatientProfileCommand(
            Guid.Empty,
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-30)
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
        var command = new CreatePatientProfileCommand(
            Guid.NewGuid(),
            "J",
            "Doe",
            DateTime.UtcNow.AddYears(-30)
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
        var command = new CreatePatientProfileCommand(
            Guid.NewGuid(),
            "John",
            "",
            DateTime.UtcNow.AddYears(-30)
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
        var command = new CreatePatientProfileCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateTime.UtcNow.AddDays(1)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage(DomainErrors.Validation.ValueCannotBeInFuture);
    }
}
