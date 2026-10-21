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
    [InlineData(AddressState.Aguascalientes, Country.Mexico)]
    [InlineData(AddressState.BajaCalifornia, Country.Mexico)]
    [InlineData(AddressState.BajaCaliforniaSur, Country.Mexico)]
    [InlineData(AddressState.Campeche, Country.Mexico)]
    [InlineData(AddressState.Chiapas, Country.Mexico)]
    [InlineData(AddressState.Chihuahua, Country.Mexico)]
    [InlineData(AddressState.CiudadDeMexico, Country.Mexico)]
    [InlineData(AddressState.Coahuila, Country.Mexico)]
    [InlineData(AddressState.Colima, Country.Mexico)]
    [InlineData(AddressState.Durango, Country.Mexico)]
    [InlineData(AddressState.EstadoDeMexico, Country.Mexico)]
    [InlineData(AddressState.Guanajuato, Country.Mexico)]
    [InlineData(AddressState.Guerrero, Country.Mexico)]
    [InlineData(AddressState.Hidalgo, Country.Mexico)]
    [InlineData(AddressState.Jalisco, Country.Mexico)]
    [InlineData(AddressState.Michoacan, Country.Mexico)]
    [InlineData(AddressState.Morelos, Country.Mexico)]
    [InlineData(AddressState.Nayarit, Country.Mexico)]
    [InlineData(AddressState.NuevoLeon, Country.Mexico)]
    [InlineData(AddressState.Oaxaca, Country.Mexico)]
    [InlineData(AddressState.Puebla, Country.Mexico)]
    [InlineData(AddressState.Queretaro, Country.Mexico)]
    [InlineData(AddressState.QuintanaRoo, Country.Mexico)]
    [InlineData(AddressState.SanLuisPotosi, Country.Mexico)]
    [InlineData(AddressState.Sinaloa, Country.Mexico)]
    [InlineData(AddressState.Sonora, Country.Mexico)]
    [InlineData(AddressState.Tabasco, Country.Mexico)]
    [InlineData(AddressState.Tamaulipas, Country.Mexico)]
    [InlineData(AddressState.Tlaxcala, Country.Mexico)]
    [InlineData(AddressState.Veracruz, Country.Mexico)]
    [InlineData(AddressState.Yucatan, Country.Mexico)]
    [InlineData(AddressState.Zacatecas, Country.Mexico)]
    [InlineData(AddressState.Alabama, Country.UnitedStates)]
    [InlineData(AddressState.Alaska, Country.UnitedStates)]
    [InlineData(AddressState.Arizona, Country.UnitedStates)]
    [InlineData(AddressState.Arkansas, Country.UnitedStates)]
    [InlineData(AddressState.California, Country.UnitedStates)]
    [InlineData(AddressState.CarolinaDelNorte, Country.UnitedStates)]
    [InlineData(AddressState.CarolinaDelSur, Country.UnitedStates)]
    [InlineData(AddressState.Colorado, Country.UnitedStates)]
    [InlineData(AddressState.Connecticut, Country.UnitedStates)]
    [InlineData(AddressState.DakotaDelNorte, Country.UnitedStates)]
    [InlineData(AddressState.DakotaDelSur, Country.UnitedStates)]
    [InlineData(AddressState.Delaware, Country.UnitedStates)]
    [InlineData(AddressState.Florida, Country.UnitedStates)]
    [InlineData(AddressState.Georgia, Country.UnitedStates)]
    [InlineData(AddressState.Hawaii, Country.UnitedStates)]
    [InlineData(AddressState.Idaho, Country.UnitedStates)]
    [InlineData(AddressState.Illinois, Country.UnitedStates)]
    [InlineData(AddressState.Indiana, Country.UnitedStates)]
    [InlineData(AddressState.Iowa, Country.UnitedStates)]
    [InlineData(AddressState.Kansas, Country.UnitedStates)]
    [InlineData(AddressState.Kentucky, Country.UnitedStates)]
    [InlineData(AddressState.Luisiana, Country.UnitedStates)]
    [InlineData(AddressState.Maine, Country.UnitedStates)]
    [InlineData(AddressState.Maryland, Country.UnitedStates)]
    [InlineData(AddressState.Massachusetts, Country.UnitedStates)]
    [InlineData(AddressState.Michigan, Country.UnitedStates)]
    [InlineData(AddressState.Minnesota, Country.UnitedStates)]
    [InlineData(AddressState.Misisipi, Country.UnitedStates)]
    [InlineData(AddressState.Misuri, Country.UnitedStates)]
    [InlineData(AddressState.Montana, Country.UnitedStates)]
    [InlineData(AddressState.Nebraska, Country.UnitedStates)]
    [InlineData(AddressState.Nevada, Country.UnitedStates)]
    [InlineData(AddressState.NuevaJersey, Country.UnitedStates)]
    [InlineData(AddressState.NuevaYork, Country.UnitedStates)]
    [InlineData(AddressState.NuevoHampshire, Country.UnitedStates)]
    [InlineData(AddressState.NuevoMexico, Country.UnitedStates)]
    [InlineData(AddressState.Ohio, Country.UnitedStates)]
    [InlineData(AddressState.Oklahoma, Country.UnitedStates)]
    [InlineData(AddressState.Oregon, Country.UnitedStates)]
    [InlineData(AddressState.Pensilvania, Country.UnitedStates)]
    [InlineData(AddressState.RhodeIsland, Country.UnitedStates)]
    [InlineData(AddressState.Tennessee, Country.UnitedStates)]
    [InlineData(AddressState.Texas, Country.UnitedStates)]
    [InlineData(AddressState.Utah, Country.UnitedStates)]
    [InlineData(AddressState.Vermont, Country.UnitedStates)]
    [InlineData(AddressState.Virginia, Country.UnitedStates)]
    [InlineData(AddressState.VirginiaOccidental, Country.UnitedStates)]
    [InlineData(AddressState.Washington, Country.UnitedStates)]
    [InlineData(AddressState.Wisconsin, Country.UnitedStates)]
    [InlineData(AddressState.Wyoming, Country.UnitedStates)]
    [InlineData(AddressState.Alberta, Country.Canada)]
    [InlineData(AddressState.ColumbiaBritanica, Country.Canada)]
    [InlineData(AddressState.IslaDelPrincipeEduardo, Country.Canada)]
    [InlineData(AddressState.Manitoba, Country.Canada)]
    [InlineData(AddressState.NuevaEscocia, Country.Canada)]
    [InlineData(AddressState.NuevoBrunswick, Country.Canada)]
    [InlineData(AddressState.Ontario, Country.Canada)]
    [InlineData(AddressState.Quebec, Country.Canada)]
    [InlineData(AddressState.Saskatchewan, Country.Canada)]
    [InlineData(AddressState.TerranovaYLabrador, Country.Canada)]
    [InlineData(AddressState.Nunavut, Country.Canada)]
    [InlineData(AddressState.TerritoriosDelNoroeste, Country.Canada)]
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
