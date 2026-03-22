using ClinicFlow.Application.Patients.Commands.AddCompleteFamilyMember;
using ClinicFlow.Domain.Enums;
using FluentValidation.TestHelper;

namespace ClinicFlow.Application.Tests.Patients.Commands.AddCompleteFamilyMember;

public class AddCompleteFamilyMemberCommandValidatorTests
{
    private readonly AddCompleteFamilyMemberCommandValidator _sut;

    public AddCompleteFamilyMemberCommandValidatorTests()
    {
        _sut = new AddCompleteFamilyMemberCommandValidator();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-10),
            "O+",
            "None",
            "None",
            "Mom",
            "555-5555",
            PatientRelationship.Child
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
        var command = new AddCompleteFamilyMemberCommand(
            Guid.Empty,
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-10),
            "O+",
            "None",
            "None",
            "Mom",
            "555-5555",
            PatientRelationship.Child
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenFirstNameIsEmptyOrTooShort()
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "J",
            "Doe",
            DateTime.UtcNow.AddYears(-10),
            "O+",
            "None",
            "None",
            "Mom",
            "555-5555",
            PatientRelationship.Child
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenLastNameIsEmptyOrTooShort()
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "",
            DateTime.UtcNow.AddYears(-10),
            "O+",
            "None",
            "None",
            "Mom",
            "555-5555",
            PatientRelationship.Child
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDateOfBirthIsInTheFuture()
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateTime.UtcNow.AddDays(1),
            "O+",
            "None",
            "None",
            "Mom",
            "555-5555",
            PatientRelationship.Child
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenBloodTypeIsEmpty()
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-10),
            "",
            "None",
            "None",
            "Mom",
            "555-5555",
            PatientRelationship.Child
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BloodType);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEmergencyContactNameIsEmpty()
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-10),
            "O+",
            "None",
            "None",
            "",
            "555-5555",
            PatientRelationship.Child
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EmergencyContactName);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEmergencyContactPhoneIsEmpty()
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-10),
            "O+",
            "None",
            "None",
            "Mom",
            "",
            PatientRelationship.Child
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EmergencyContactPhone);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenRelationshipIsInvalidEnum()
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            DateTime.UtcNow.AddYears(-10),
            "O+",
            "None",
            "None",
            "Mom",
            "555-5555",
            (PatientRelationship)999
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Relationship);
    }
}
