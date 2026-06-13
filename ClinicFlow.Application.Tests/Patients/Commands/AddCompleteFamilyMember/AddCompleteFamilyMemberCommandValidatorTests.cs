using ClinicFlow.Application.Patients.Commands.AddCompleteFamilyMember;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;
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

    [Fact]
    public void Validate_ShouldHaveError_WhenFirstNameIsTooLong()
    {
        // Arrange
        var firstName = new string('A', PersonName.MaximumLength + 1);
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            firstName,
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
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
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
    public void Validate_ShouldHaveError_WhenLastNameIsTooLong()
    {
        // Arrange
        var lastName = new string('A', PersonName.MaximumLength + 1);
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            lastName,
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
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
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

    [Fact]
    public void Validate_ShouldHaveError_WhenEmergencyContactNameIsTooShort()
    {
        // Arrange
        var emergencyContactName = new string('A', PersonName.MinimumLength - 1);
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30),
            "O+",
            "Peanut allergy",
            "Asthma",
            emergencyContactName,
            "555-0199",
            PatientRelationship.Spouse
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
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30),
            "O+",
            "Peanut allergy",
            "Asthma",
            emergencyContactName,
            "555-0199",
            PatientRelationship.Spouse
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
    public void Validate_ShouldHaveError_WhenEmergencyContactPhoneIsTooShort()
    {
        // Arrange
        var emergencyContactPhone = new string('1', PhoneNumber.MinimumLength - 1);
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30),
            "O+",
            "Peanut allergy",
            "Asthma",
            "Jane Doe",
            emergencyContactPhone,
            PatientRelationship.Spouse
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
        var emergencyContactPhone = new string('1', PhoneNumber.MaximumLength + 1);
        var command = new AddCompleteFamilyMemberCommand(
            Guid.NewGuid(),
            "John",
            "Doe",
            _fakeTime.GetUtcNow().UtcDateTime.AddYears(-30),
            "O+",
            "Peanut allergy",
            "Asthma",
            "Jane Doe",
            emergencyContactPhone,
            PatientRelationship.Spouse
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EmergencyContactPhone)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
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
