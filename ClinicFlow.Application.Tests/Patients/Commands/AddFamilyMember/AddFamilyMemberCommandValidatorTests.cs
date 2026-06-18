using ClinicFlow.Application.Patients.Commands.AddFamilyMember;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Application.Tests.Patients.Commands.AddFamilyMember;

public class AddFamilyMemberCommandValidatorTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly AddFamilyMemberCommandValidator _sut;

    public AddFamilyMemberCommandValidatorTests()
    {
        _sut = new AddFamilyMemberCommandValidator(_fakeTime);
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new AddFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse
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
        var command = new AddFamilyMemberCommand(
            Guid.Empty,
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse
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
        var command = new AddFamilyMemberCommand(
            Guid.NewGuid(),
            firstName!,
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse
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
        var command = new AddFamilyMemberCommand(
            Guid.NewGuid(),
            "J",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse
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
        var firstName = new string('A', PersonName.MaximumLength + 1);
        var command = new AddFamilyMemberCommand(
            Guid.NewGuid(),
            firstName,
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse
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
        var command = new AddFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            lastName!,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse
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
        var command = new AddFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "D",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse
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
        var lastName = new string('A', PersonName.MaximumLength + 1);
        var command = new AddFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            lastName,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse
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
        var command = new AddFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            PatientRelationship.Spouse
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage(DomainErrors.Validation.ValueCannotBeInFuture);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenRelationshipIsInvalid()
    {
        // Arrange
        var command = new AddFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            (PatientRelationship)999
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Relationship)
            .WithErrorMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenRelationshipIsSelf()
    {
        // Arrange
        var command = new AddFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Self
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Relationship)
            .WithErrorMessage(DomainErrors.Patient.CannotBeSelf);
    }
}
