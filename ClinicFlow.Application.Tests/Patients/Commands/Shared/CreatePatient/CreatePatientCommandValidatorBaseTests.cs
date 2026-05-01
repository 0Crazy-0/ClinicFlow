using ClinicFlow.Application.Patients.Commands.Shared.CreatePatient;
using ClinicFlow.Domain.Common;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Application.Tests.Patients.Commands.Shared.CreatePatient;

public record DummyCreatePatientCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    DateTime DateOfBirth
) : ICreatePatientCommand;

public class DummyCreatePatientCommandValidator(TimeProvider timeProvider)
    : CreatePatientCommandValidatorBase<DummyCreatePatientCommand>(timeProvider) { }

public class CreatePatientCommandValidatorBaseTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly DummyCreatePatientCommandValidator _sut;

    public CreatePatientCommandValidatorBaseTests()
    {
        _sut = new DummyCreatePatientCommandValidator(_fakeTime);
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new DummyCreatePatientCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)
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
        var command = new DummyCreatePatientCommand(
            Guid.Empty,
            "John",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)
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
        var command = new DummyCreatePatientCommand(
            Guid.NewGuid(),
            "J",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)
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
        var command = new DummyCreatePatientCommand(
            Guid.NewGuid(),
            "John",
            "",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)
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
        var command = new DummyCreatePatientCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage(DomainErrors.Validation.ValueCannotBeInFuture);
    }
}
