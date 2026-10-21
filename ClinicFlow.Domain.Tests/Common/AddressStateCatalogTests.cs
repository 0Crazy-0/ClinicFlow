using AwesomeAssertions;
using ClinicFlow.Domain.Common;
using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Tests.Common;

public class AddressStateCatalogTests
{
    [Fact]
    public void GetStates_ShouldReturnThirtyTwoStates_WhenCountryIsMexico() =>
        AddressStateCatalog.GetStates(Country.Mexico).Should().HaveCount(32);

    [Fact]
    public void GetStates_ShouldReturnFiftyStates_WhenCountryIsUnitedStates() =>
        AddressStateCatalog.GetStates(Country.UnitedStates).Should().HaveCount(50);

    [Fact]
    public void GetStates_ShouldReturnThirteenStates_WhenCountryIsCanada() =>
        AddressStateCatalog.GetStates(Country.Canada).Should().HaveCount(13);

    [Fact]
    public void GetStates_ShouldCoverEveryAddressState_WhenAllCountriesAreCombined()
    {
        // Arrange
        var cataloguedStates = AddressStateCatalog
            .GetStates(Country.Mexico)
            .Concat(AddressStateCatalog.GetStates(Country.UnitedStates))
            .Concat(AddressStateCatalog.GetStates(Country.Canada));

        // Assert
        cataloguedStates.Should().BeEquivalentTo(Enum.GetValues<AddressState>());
    }

    [Fact]
    public void GetStates_ShouldNotRepeatStates_WhenAllCountriesAreCombined()
    {
        // Arrange
        var cataloguedStates = AddressStateCatalog
            .GetStates(Country.Mexico)
            .Concat(AddressStateCatalog.GetStates(Country.UnitedStates))
            .Concat(AddressStateCatalog.GetStates(Country.Canada));

        // Assert
        cataloguedStates.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void GetStates_ShouldThrowException_WhenCountryIsUndefined()
    {
        // Arrange & Act
        var act = () => AddressStateCatalog.GetStates((Country)999);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }

    [Theory]
    [InlineData(AddressState.CiudadDeMexico, Country.Mexico)]
    [InlineData(AddressState.EstadoDeMexico, Country.Mexico)]
    [InlineData(AddressState.Yucatan, Country.Mexico)]
    [InlineData(AddressState.Pensilvania, Country.UnitedStates)]
    [InlineData(AddressState.California, Country.UnitedStates)]
    [InlineData(AddressState.Hawaii, Country.UnitedStates)]
    [InlineData(AddressState.Ontario, Country.Canada)]
    [InlineData(AddressState.Quebec, Country.Canada)]
    [InlineData(AddressState.Yukon, Country.Canada)]
    public void GetCountry_ShouldReturnCountry_WhenStateIsDefined(
        AddressState state,
        Country expectedCountry
    ) => AddressStateCatalog.GetCountry(state).Should().Be(expectedCountry);

    [Fact]
    public void GetCountry_ShouldThrowException_WhenStateIsUndefined()
    {
        // Arrange & Act
        var act = () => AddressStateCatalog.GetCountry((AddressState)999);

        // Assert
        act.Should()
            .Throw<DomainValidationException>()
            .WithMessage(DomainErrors.Validation.InvalidEnumValue);
    }
}
