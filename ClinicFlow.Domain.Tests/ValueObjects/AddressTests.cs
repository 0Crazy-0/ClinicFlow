using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;
using ClinicFlow.Domain.ValueObjects;

namespace ClinicFlow.Domain.Tests.ValueObjects;

public class AddressTests
{
    [Theory]
    [InlineData(
        "Av. Juárez s/n, Centro Histórico, Cuauhtémoc",
        AddressState.CiudadDeMexico,
        "06050",
        Country.Mexico
    )]
    [InlineData("901 Swinehart Rd", AddressState.Pensilvania, "10004", Country.UnitedStates)]
    [InlineData("150 Elgin Street", AddressState.Ontario, "K2P 1L4", Country.Canada)]
    public void Create_ShouldCreateAddress_WhenValidParameters(
        string line,
        AddressState state,
        string postalCode,
        Country country
    )
    {
        // Arrange & Act
        var address = Address.Create(line, state, postalCode, country);

        // Assert
        address.Line.Should().Be(line);
        address.State.Should().Be(state);
        address.PostalCode.Should().Be(postalCode);
        address.Country.Should().Be(country);
    }

    [Fact]
    public void Create_ShouldNormalizePostalCode_WhenCanadianPostalCodeHasNoSpace()
    {
        // Arrange & Act
        var address = Address.Create(
            "150 Elgin Street",
            AddressState.Ontario,
            "k2p1l4",
            Country.Canada
        );

        // Assert
        address.PostalCode.Should().Be("K2P 1L4");
    }

    [Fact]
    public void Create_ShouldKeepPostalCodeFormat_WhenUnitedStatesPostalCodeHasPlusFour()
    {
        // Arrange & Act
        var address = Address.Create(
            "901 Swinehart Rd",
            AddressState.Pensilvania,
            "10004-1234",
            Country.UnitedStates
        );

        // Assert
        address.PostalCode.Should().Be("10004-1234");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        // Arrange & Act
        var address = Address.Create(
            "  Av. Juárez s/n  ",
            AddressState.CiudadDeMexico,
            "  06050  ",
            Country.Mexico
        );

        // Assert
        address.Line.Should().Be("Av. Juárez s/n");
        address.PostalCode.Should().Be("06050");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenLineIsEmpty(string? line)
    {
        // Arrange & Act
        var act = () => Address.Create(line!, AddressState.CiudadDeMexico, "06050", Country.Mexico);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenPostalCodeIsEmpty(string? postalCode)
    {
        // Arrange & Act
        var act = () =>
            Address.Create(
                "Av. Juárez s/n",
                AddressState.CiudadDeMexico,
                postalCode!,
                Country.Mexico
            );

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.ValueRequired);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenStateIsUndefined()
    {
        // Arrange & Act
        var act = () =>
            Address.Create("Av. Juárez s/n", (AddressState)999, "06050", Country.Mexico);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenCountryIsUndefined()
    {
        // Arrange & Act
        var act = () =>
            Address.Create("Av. Juárez s/n", AddressState.CiudadDeMexico, "06050", (Country)999);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenLineIsTooLong()
    {
        // Arrange
        var line = new string('a', Address.LineMaximumLength + 1);

        // Act
        var act = () => Address.Create(line, AddressState.CiudadDeMexico, "06050", Country.Mexico);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void Create_ShouldSucceed_WhenLineLengthEqualsMaximumLength()
    {
        // Arrange
        var line = new string('a', Address.LineMaximumLength);

        // Act
        var address = Address.Create(line, AddressState.CiudadDeMexico, "06050", Country.Mexico);

        // Assert
        address.Line.Should().Be(line);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenPostalCodeIsTooLong()
    {
        // Arrange
        var postalCode = new string('1', Address.PostalCodeMaximumLength + 1);

        // Act
        var act = () =>
            Address.Create(
                "Av. Juárez s/n",
                AddressState.CiudadDeMexico,
                postalCode,
                Country.Mexico
            );

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Validation.ValueTooLong);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenStateIsUndefined_EvenWhenLineIsTooLong()
    {
        // Arrange
        var line = new string('a', Address.LineMaximumLength + 1);

        // Act
        var act = () => Address.Create(line, (AddressState)999, "06050", Country.Mexico);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Theory]
    [InlineData("Av. Juárez<s/n")]
    [InlineData("Av. Juárez\ns/n")]
    [InlineData("Av. Juárez\ts/n")]
    public void Create_ShouldThrowException_WhenLineContainsInvalidCharacters(string line)
    {
        // Arrange & Act
        var act = () => Address.Create(line, AddressState.CiudadDeMexico, "06050", Country.Mexico);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Address.InvalidLineFormat);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenStateDoesNotBelongToCountry()
    {
        // Arrange & Act
        var act = () =>
            Address.Create(
                "901 Swinehart Rd",
                AddressState.CiudadDeMexico,
                "06050",
                Country.UnitedStates
            );

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Address.StateCountryMismatch);
    }

    [Theory]
    [InlineData("1234")]
    [InlineData("060500")]
    [InlineData("K2P 1L4")]
    [InlineData("06050-123")]
    [InlineData("١٢٣٤٥")]
    public void Create_ShouldThrowException_WhenMexicanPostalCodeIsInvalid(string postalCode)
    {
        // Arrange & Act
        var act = () =>
            Address.Create(
                "Av. Juárez s/n",
                AddressState.CiudadDeMexico,
                postalCode,
                Country.Mexico
            );

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Address.InvalidPostalCode);
    }

    [Theory]
    [InlineData("1234")]
    [InlineData("100044")]
    [InlineData("K2P 1L4")]
    [InlineData("10004-123")]
    [InlineData("١٢٣٤٥")]
    public void Create_ShouldThrowException_WhenUnitedStatesPostalCodeIsInvalid(string postalCode)
    {
        // Arrange & Act
        var act = () =>
            Address.Create(
                "901 Swinehart Rd",
                AddressState.Pensilvania,
                postalCode,
                Country.UnitedStates
            );

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Address.InvalidPostalCode);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("K2P 1L")]
    [InlineData("D2P 1L4")]
    [InlineData("K2P D1L4")]
    [InlineData("K2P1L4X")]
    [InlineData("K٢P 1L4")]
    public void Create_ShouldThrowException_WhenCanadianPostalCodeIsInvalid(string postalCode)
    {
        // Arrange & Act
        var act = () =>
            Address.Create("150 Elgin Street", AddressState.Ontario, postalCode, Country.Canada);

        // Assert
        act.Should()
            .Throw<BusinessRuleValidationException>()
            .WithMessage(DomainErrors.Address.InvalidPostalCode);
    }
}
