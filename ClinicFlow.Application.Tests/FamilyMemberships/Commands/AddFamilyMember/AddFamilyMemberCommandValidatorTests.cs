using ClinicFlow.Application.FamilyMemberships.Commands.AddFamilyMember;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Application.Tests.FamilyMemberships.Commands.AddFamilyMember;

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
            Guid.CreateVersion7(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            BiologicalSex.Female,
            "123 Main Street",
            AddressState.California,
            "90210",
            Country.UnitedStates
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
            PatientRelationship.Spouse,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            BiologicalSex.Female,
            "123 Main Street",
            AddressState.California,
            "90210",
            Country.UnitedStates
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
            Guid.CreateVersion7(),
            firstName!,
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            BiologicalSex.Female,
            "123 Main Street",
            AddressState.California,
            "90210",
            Country.UnitedStates
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
            Guid.CreateVersion7(),
            "J",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            BiologicalSex.Female,
            "123 Main Street",
            AddressState.California,
            "90210",
            Country.UnitedStates
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
            Guid.CreateVersion7(),
            firstName,
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            BiologicalSex.Female,
            "123 Main Street",
            AddressState.California,
            "90210",
            Country.UnitedStates
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
            Guid.CreateVersion7(),
            "John",
            lastName!,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            BiologicalSex.Female,
            "123 Main Street",
            AddressState.California,
            "90210",
            Country.UnitedStates
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
            Guid.CreateVersion7(),
            "John",
            "D",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            BiologicalSex.Female,
            "123 Main Street",
            AddressState.California,
            "90210",
            Country.UnitedStates
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
            Guid.CreateVersion7(),
            "John",
            lastName,
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            BiologicalSex.Female,
            "123 Main Street",
            AddressState.California,
            "90210",
            Country.UnitedStates
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
            Guid.CreateVersion7(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            PatientRelationship.Spouse,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            BiologicalSex.Female,
            "123 Main Street",
            AddressState.California,
            "90210",
            Country.UnitedStates
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
            Guid.CreateVersion7(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            (PatientRelationship)999,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            BiologicalSex.Female,
            "123 Main Street",
            AddressState.California,
            "90210",
            Country.UnitedStates
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
            Guid.CreateVersion7(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Self,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            BiologicalSex.Female,
            "123 Main Street",
            AddressState.California,
            "90210",
            Country.UnitedStates
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Relationship)
            .WithErrorMessage(DomainErrors.Patient.CannotBeSelf);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAccessLevelIsInvalid()
    {
        // Arrange
        var command = new AddFamilyMemberCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse,
            LegalAuthorityType.None,
            (FamilyMembershipAccessLevel)967,
            BiologicalSex.Female,
            "123 Main Street",
            AddressState.California,
            "90210",
            Country.UnitedStates
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AccessLevel)
            .WithErrorMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAccessLevelIsUnspecified()
    {
        // Arrange
        var command = new AddFamilyMemberCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Unspecified,
            BiologicalSex.Female,
            "123 Main Street",
            AddressState.California,
            "90210",
            Country.UnitedStates
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AccessLevel)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenLegalAuthorityIsInvalid()
    {
        // Arrange
        var command = new AddFamilyMemberCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse,
            (LegalAuthorityType)999,
            FamilyMembershipAccessLevel.Full,
            BiologicalSex.Female,
            "123 Main Street",
            AddressState.California,
            "90210",
            Country.UnitedStates
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.LegalAuthority)
            .WithErrorMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenBiologicalSexIsInvalid()
    {
        // Arrange
        var command = new AddFamilyMemberCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            (BiologicalSex)999,
            "123 Main Street",
            AddressState.California,
            "90210",
            Country.UnitedStates
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.SexAtBirth)
            .WithErrorMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenBiologicalSexIsUnspecified()
    {
        // Arrange
        var command = new AddFamilyMemberCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            BiologicalSex.Unspecified,
            "123 Main Street",
            AddressState.California,
            "90210",
            Country.UnitedStates
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.SexAtBirth)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenAddressLineIsEmpty(string? addressLine)
    {
        // Arrange
        var command = new AddFamilyMemberCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            BiologicalSex.Male,
            addressLine!,
            AddressState.California,
            "90210",
            Country.UnitedStates
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AddressLine)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAddressLineIsTooLong()
    {
        // Arrange
        var command = new AddFamilyMemberCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            BiologicalSex.Male,
            new string('A', Address.LineMaximumLength + 1),
            AddressState.California,
            "90210",
            Country.UnitedStates
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AddressLine)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAddressStateIsInvalid()
    {
        // Arrange
        var command = new AddFamilyMemberCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            BiologicalSex.Male,
            "123 Main Street",
            (AddressState)999,
            "90210",
            Country.UnitedStates
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AddressState)
            .WithErrorMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenAddressPostalCodeIsEmpty(string? postalCode)
    {
        // Arrange
        var command = new AddFamilyMemberCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            BiologicalSex.Male,
            "123 Main Street",
            AddressState.California,
            postalCode!,
            Country.UnitedStates
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AddressPostalCode)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAddressPostalCodeIsTooLong()
    {
        // Arrange
        var command = new AddFamilyMemberCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            BiologicalSex.Male,
            "123 Main Street",
            AddressState.California,
            new string('9', Address.PostalCodeMaximumLength + 1),
            Country.UnitedStates
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AddressPostalCode)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAddressCountryIsInvalid()
    {
        // Arrange
        var command = new AddFamilyMemberCommand(
            Guid.CreateVersion7(),
            "John",
            "Doe",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddYears(-30)),
            PatientRelationship.Spouse,
            LegalAuthorityType.None,
            FamilyMembershipAccessLevel.Full,
            BiologicalSex.Male,
            "123 Main Street",
            AddressState.California,
            "90210",
            (Country)967
        );

        // Act
        var result = _sut.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AddressCountry)
            .WithErrorMessage(DomainErrors.Validation.InvalidEnumValue);
    }
}
