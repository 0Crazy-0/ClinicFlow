using ClinicFlow.Application.Patients.Commands.UpdatePatientProfile;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.ValueObjects;
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
            Guid.CreateVersion7(),
            "O+",
            "None",
            "None",
            "Jane",
            "María",
            "Doe",
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenEmergencyContactMiddleNameIsNull()
    {
        // Arrange
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            "O+",
            "None",
            "None",
            "Jane",
            null,
            "Doe",
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
            "Jane",
            "María",
            "Doe",
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PatientId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenBloodTypeIsEmpty(string? bloodType)
    {
        // Arrange
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            bloodType!,
            "None",
            "None",
            "Jane",
            "María",
            "Doe",
            "555-5555"
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
    public void Validate_ShouldHaveError_WhenEmergencyContactFirstNameIsEmpty(string? firstName)
    {
        // Arrange
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            "O+",
            "None",
            "None",
            firstName!,
            "María",
            "Doe",
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EmergencyContactFirstName)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEmergencyContactFirstNameIsTooShort()
    {
        // Arrange
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            "O+",
            "None",
            "None",
            "A",
            "María",
            "Doe",
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EmergencyContactFirstName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEmergencyContactFirstNameIsTooLong()
    {
        // Arrange
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            "O+",
            "None",
            "None",
            new string('A', PersonName.MaximumLength + 1),
            "María",
            "Doe",
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EmergencyContactFirstName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenEmergencyContactMiddleNameIsEmpty(string? middleName)
    {
        // Arrange
        // Empty and whitespace violate NotEmpty inside the When block; the null case
        // bypasses the When condition and is covered by the ShouldBeValid test above.
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            "O+",
            "None",
            "None",
            "Jane",
            middleName,
            "Doe",
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EmergencyContactMiddleName)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEmergencyContactMiddleNameIsTooShort()
    {
        // Arrange
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            "O+",
            "None",
            "None",
            "Jane",
            "A",
            "Doe",
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EmergencyContactMiddleName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEmergencyContactMiddleNameIsTooLong()
    {
        // Arrange
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            "O+",
            "None",
            "None",
            "Jane",
            new string('A', PersonName.MaximumLength + 1),
            "Doe",
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EmergencyContactMiddleName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenEmergencyContactLastNameIsEmpty(string? lastName)
    {
        // Arrange
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            "O+",
            "None",
            "None",
            "Jane",
            "María",
            lastName!,
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EmergencyContactLastName)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEmergencyContactLastNameIsTooShort()
    {
        // Arrange
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            "O+",
            "None",
            "None",
            "Jane",
            "María",
            "A",
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EmergencyContactLastName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEmergencyContactLastNameIsTooLong()
    {
        // Arrange
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            "O+",
            "None",
            "None",
            "Jane",
            "María",
            new string('A', PersonName.MaximumLength + 1),
            "555-5555"
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EmergencyContactLastName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenEmergencyContactPhoneIsEmpty(string? phone)
    {
        // Arrange
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            "O+",
            "None",
            "None",
            "Jane",
            "María",
            "Doe",
            phone!
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
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            "O+",
            "None",
            "None",
            "Jane",
            "María",
            "Doe",
            new string('1', PhoneNumber.MinimumLength - 1)
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
        var command = new UpdatePatientProfileCommand(
            Guid.CreateVersion7(),
            "O+",
            "None",
            "None",
            "Jane",
            "María",
            "Doe",
            "+" + new string('1', PhoneNumber.MaximumLength)
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.EmergencyContactPhone)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }
}
