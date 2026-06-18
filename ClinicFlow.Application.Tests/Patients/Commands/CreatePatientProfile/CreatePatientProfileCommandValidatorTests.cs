using ClinicFlow.Application.Patients.Commands.CreatePatientProfile;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.ValueObjects;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Application.Tests.Patients.Commands.CreatePatientProfile;

public class CreatePatientProfileCommandValidatorTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly CreatePatientProfileCommandValidator _sut;

    public CreatePatientProfileCommandValidatorTests()
    {
        _sut = new CreatePatientProfileCommandValidator(_fakeTime);
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new CreatePatientProfileCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30))
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
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30))
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenFirstNameIsEmpty(string? firstName)
    {
        // Arrange
        var command = new CreatePatientProfileCommand(
            Guid.NewGuid(),
            firstName!,
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30))
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenFirstNameIsTooShort()
    {
        // Arrange
        var command = new CreatePatientProfileCommand(
            Guid.NewGuid(),
            "J",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30))
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenFirstNameIsTooLong()
    {
        // Arrange
        var command = new CreatePatientProfileCommand(
            Guid.NewGuid(),
            new string('A', PersonName.MaximumLength + 1),
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30))
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenLastNameIsEmpty(string? lastName)
    {
        // Arrange
        var command = new CreatePatientProfileCommand(
            Guid.NewGuid(),
            "John",
            lastName!,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30))
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenLastNameIsTooShort()
    {
        // Arrange
        var command = new CreatePatientProfileCommand(
            Guid.NewGuid(),
            "John",
            "D",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30))
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenLastNameIsTooLong()
    {
        // Arrange
        var command = new CreatePatientProfileCommand(
            Guid.NewGuid(),
            "John",
            new string('A', PersonName.MaximumLength + 1),
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30))
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDateOfBirthIsInTheFuture()
    {
        // Arrange
        var command = new CreatePatientProfileCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1))
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage(DomainErrors.Validation.ValueCannotBeInFuture);
    }
}
