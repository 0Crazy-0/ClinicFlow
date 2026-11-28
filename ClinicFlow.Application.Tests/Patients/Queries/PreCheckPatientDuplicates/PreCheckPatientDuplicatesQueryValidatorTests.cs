using ClinicFlow.Application.Patients.Queries.PreCheckPatientDuplicates;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.ValueObjects;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;

namespace ClinicFlow.Application.Tests.Patients.Queries.PreCheckPatientDuplicates;

public class PreCheckPatientDuplicatesQueryValidatorTests
{
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly PreCheckPatientDuplicatesQueryValidator _sut;

    public PreCheckPatientDuplicatesQueryValidatorTests()
    {
        _sut = new PreCheckPatientDuplicatesQueryValidator(_fakeTime);
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenAllPropertiesAreProvidedAndValid()
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "Fluttershy",
            "Tests",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            "Ponyville",
            AddressState.California,
            "90210",
            Country.UnitedStates,
            "fluttershy@clinicflow.test",
            "+1 555 1234567",
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldBeValid_WhenSsnLast4IsNull()
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "Rainbow",
            "Dash",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            "Ponyville",
            AddressState.California,
            "90210",
            Country.UnitedStates,
            "rainbow@clinicflow.test",
            "+1 555 1234567",
            null
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ShouldNotHaveError_WhenSsnLast4HasValidFormat()
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "Sunset",
            "Shimmer",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            "Canterlot City",
            AddressState.California,
            "90210",
            Country.UnitedStates,
            "sunset@clinicflow.test",
            "+1 555 1234567",
            "0000"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SsnLast4);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenReceptionistUserIdIsEmpty()
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.Empty,
            "Pinkie",
            "Pie",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            "Sugarcube Corner",
            AddressState.California,
            "90210",
            Country.UnitedStates,
            "pinkie@clinicflow.test",
            "+1 555 1234567",
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.ReceptionistUserId)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenFirstNameIsEmpty(string? firstName)
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            firstName!,
            "Tests",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Male,
            "Crazy house",
            AddressState.California,
            "90210",
            Country.UnitedStates,
            "crazy@clinicflow.test",
            "+1 555 1234567",
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenFirstNameIsTooShort()
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "A",
            "Tests",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            "Test house",
            AddressState.California,
            "90210",
            Country.UnitedStates,
            "test@clinicflow.test",
            "+1 555 1234567",
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenFirstNameIsTooLong()
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            new string('A', PersonName.MaximumLength + 1),
            "Tests",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            "Test house",
            AddressState.California,
            "90210",
            Country.UnitedStates,
            "test@clinicflow.test",
            "+1 555 1234567",
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

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
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "Rarity",
            lastName!,
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            "Boutique",
            AddressState.California,
            "90210",
            Country.UnitedStates,
            "rarity@clinicflow.test",
            "+1 555 1234567",
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenLastNameIsTooShort()
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "Twilight",
            "S",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            "Ponyville",
            AddressState.California,
            "90210",
            Country.UnitedStates,
            "twilight@clinicflow.test",
            "+1 555 1234567",
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenLastNameIsTooLong()
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "Discord",
            new string('A', PersonName.MaximumLength + 1),
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            "Crazy house",
            AddressState.California,
            "90210",
            Country.UnitedStates,
            "discord@clinicflow.test",
            "+1 555 1234567",
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenDateOfBirthIsInTheFuture()
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "Apple Bloom",
            "Tests",
            DateOnly.FromDateTime(_fakeTime.GetUtcNow().UtcDateTime.AddDays(1)),
            BiologicalSex.Female,
            "Sweet Apple Acres",
            AddressState.California,
            "90210",
            Country.UnitedStates,
            "applebloom@clinicflow.test",
            "+1 555 1234567",
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage(DomainErrors.Validation.ValueCannotBeInFuture);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenSexAtBirthIsInvalid()
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "Changeling",
            "Tests",
            new DateOnly(1999, 5, 15),
            (BiologicalSex)999,
            "Changeling Kingdom",
            AddressState.California,
            "90210",
            Country.UnitedStates,
            "changeling@clinicflow.test",
            "+1 555 1234567",
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.SexAtBirth)
            .WithErrorMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenSexAtBirthIsUnspecified()
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "Trixie",
            "Tests",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Unspecified,
            "The Caravan",
            AddressState.California,
            "90210",
            Country.UnitedStates,
            "thegreatandpowerfultrixie@clinicflow.test",
            "+1 555 1234567",
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.SexAtBirth)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenAddressLineIsEmpty(string? addressLine)
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "Derpy",
            "Tests",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            addressLine!,
            AddressState.California,
            "90210",
            Country.UnitedStates,
            "derpy@clinicflow.test",
            "+1 555 1234567",
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AddressLine)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAddressLineIsTooLong()
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "Princess Celestia",
            "Tests",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            new string('A', Address.LineMaximumLength + 1),
            AddressState.California,
            "90210",
            Country.UnitedStates,
            "celestia@clinicflow.test",
            "+1 555 1234567",
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AddressLine)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAddressStateIsInvalid()
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "Princess Luna",
            "Tests",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            "Moon",
            (AddressState)999,
            "90210",
            Country.UnitedStates,
            "luna@clinicflow.test",
            "+1 555 1234567",
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

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
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "DJ pony-3",
            "Tests",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            "Ponyville",
            AddressState.California,
            postalCode!,
            Country.UnitedStates,
            "djpony@clinicflow.test",
            "+1 555 1234567",
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AddressPostalCode)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAddressPostalCodeIsTooLong()
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "Octavia",
            "Melody",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            "Ponyville",
            AddressState.California,
            new string('1', Address.PostalCodeMaximumLength + 1),
            Country.UnitedStates,
            "octavia@clinicflow.test",
            "+1 555 1234567",
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AddressPostalCode)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenAddressCountryIsInvalid()
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "Applejack",
            "Tests",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            "Sweet Apple Acres",
            AddressState.California,
            "90210",
            (Country)999,
            "applejack@clinicflow.test",
            "+1 555 1234567",
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AddressCountry)
            .WithErrorMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenEmailIsEmpty(string? email)
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "Lyra",
            "Tests",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            "Ponyville",
            AddressState.California,
            "90210",
            Country.UnitedStates,
            email!,
            "+1 555 1234567",
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEmailHasInvalidFormat()
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "Maud",
            "Pie",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            "Ponyville",
            AddressState.California,
            "90210",
            Country.UnitedStates,
            "not-an-email",
            "+1 555 1234567",
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenEmailIsTooLong()
    {
        // Arrange

        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "Marble",
            "Pie",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            "Ponyville",
            AddressState.California,
            "90210",
            Country.UnitedStates,
            new string('m', EmailAddress.MaximumLength + 1),
            "+1 555 1234567",
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_ShouldHaveError_WhenPhoneIsEmpty(string? phone)
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "Bon Bon",
            "Tets",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            "Ponyville",
            AddressState.California,
            "90210",
            Country.UnitedStates,
            "lyraandbonbon@clinicflow.test",
            phone!,
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Phone)
            .WithErrorMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPhoneIsTooShort()
    {
        // Arrange
        var phone = new string('1', PhoneNumber.MinimumLength - 1);
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "Cloudy Quartz",
            "Pie",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            "Rock Farm",
            AddressState.California,
            "90210",
            Country.UnitedStates,
            "cloudyquartz@clinicflow.test",
            phone,
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Phone)
            .WithErrorMessage(DomainErrors.Validation.ValueTooShort);
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenPhoneIsTooLong()
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "Igneous Rock",
            "Pie",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            "Rock Farm",
            AddressState.California,
            "90210",
            Country.UnitedStates,
            "igneoustrock@clinicflow.test",
            new string('1', PhoneNumber.MaximumLength + 1),
            "1234"
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.Phone)
            .WithErrorMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("12a4")]
    [InlineData("abcd")]
    [InlineData("1234\n")]
    [InlineData("1234\r\n")]
    public void Validate_ShouldHaveError_WhenSsnLast4HasInvalidFormat(string ssnLast4)
    {
        // Arrange
        var query = new PreCheckPatientDuplicatesQuery(
            Guid.CreateVersion7(),
            "Starlight",
            "Glimmer",
            new DateOnly(1999, 5, 15),
            BiologicalSex.Female,
            "Ponyville",
            AddressState.California,
            "90210",
            Country.UnitedStates,
            "starlight@clinicflow.test",
            "+1 555 1234567",
            ssnLast4
        );

        // Act
        var result = _sut.TestValidate(query);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.SsnLast4)
            .WithErrorMessage(DomainErrors.Validation.InvalidValue);
    }
}
