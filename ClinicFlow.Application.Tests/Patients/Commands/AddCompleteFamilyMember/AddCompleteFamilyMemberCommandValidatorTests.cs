using ClinicFlow.Application.Patients.Commands.AddCompleteFamilyMember;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Application.Tests.Patients.Commands.AddCompleteFamilyMember;

public class AddCompleteFamilyMemberCommandValidatorTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly AddCompleteFamilyMemberCommandValidator _sut;

    public AddCompleteFamilyMemberCommandValidatorTests()
    {
        _sut = new AddCompleteFamilyMemberCommandValidator(_fakeTime);
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30),
            "O+",
            "Peanut allergy",
            "Asthma",
            "Jane Doe",
            "555-0199",
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
        var command = new AddCompleteFamilyMemberCommand(
            Guid.Empty,
            "John",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30),
            "O+",
            "Peanut allergy",
            "Asthma",
            "Jane Doe",
            "555-0199",
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
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            firstName!,
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30),
            "O+",
            "Peanut allergy",
            "Asthma",
            "Jane Doe",
            "555-0199",
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
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "J",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30),
            "O+",
            "Peanut allergy",
            "Asthma",
            "Jane Doe",
            "555-0199",
            PatientRelationship.Spouse
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenLastNameIsEmpty(string? lastName)
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            lastName!,
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30),
            "O+",
            "Peanut allergy",
            "Asthma",
            "Jane Doe",
            "555-0199",
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
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "D",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30),
            "O+",
            "Peanut allergy",
            "Asthma",
            "Jane Doe",
            "555-0199",
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
    public void Validate_ShouldHaveError_WhenDateOfBirthIsInTheFuture()
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddDays(1),
            "O+",
            "Peanut allergy",
            "Asthma",
            "Jane Doe",
            "555-0199",
            PatientRelationship.Spouse
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage(DomainErrors.Validation.ValueCannotBeInFuture);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenBloodTypeIsEmpty(string? bloodType)
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30),
            bloodType!,
            "Peanut allergy",
            "Asthma",
            "Jane Doe",
            "555-0199",
            PatientRelationship.Spouse
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
    public void Validate_ShouldHaveError_WhenEmergencyContactNameIsEmpty(string? name)
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30),
            "O+",
            "Peanut allergy",
            "Asthma",
            name!,
            "555-0199",
            PatientRelationship.Spouse
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EmergencyContactName)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenEmergencyContactPhoneIsEmpty(string? phone)
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30),
            "O+",
            "Peanut allergy",
            "Asthma",
            "Jane Doe",
            phone!,
            PatientRelationship.Spouse
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EmergencyContactPhone)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenRelationshipIsInvalid()
    {
        // Arrange
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30),
            "O+",
            "Peanut allergy",
            "Asthma",
            "Jane Doe",
            "555-0199",
            (PatientRelationship)999
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Relationship)
            .WithErrorMessage(DomainErrors.Validation.InvalidEnumValue);
    }
}
