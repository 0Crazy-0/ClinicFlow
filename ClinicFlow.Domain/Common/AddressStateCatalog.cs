using ClinicFlow.Domain.Enums;
using ClinicFlow.Domain.Exceptions.Base;

namespace ClinicFlow.Domain.Common;

/// <summary>
/// Provides the country each address state belongs to and the official
/// display names used to present the states and countries to the user.
/// </summary>
public static class AddressStateCatalog
{
    /// <summary>
    /// Returns the states, provinces and territories that belong to the given country.
    /// </summary>
    public static IReadOnlyList<AddressState> GetStates(Country country)
    {
        Guard.IsDefined(country);

        return country switch
        {
            Country.Mexico => [.. MexicoEntries.Select(entry => entry.State)],
            Country.Canada => [.. CanadaEntries.Select(entry => entry.State)],
            Country.UnitedStates => [.. UnitedStatesEntries.Select(entry => entry.State)],
            _ => throw new DomainValidationException(DomainErrors.Address.CountryNotInCatalog),
        };
    }

    /// <summary>
    /// Returns the country the given state belongs to.
    /// </summary>
    public static Country GetCountry(AddressState state)
    {
        Guard.IsDefined(state);

        if (MexicoEntries.Any(entry => entry.State == state))
            return Country.Mexico;

        if (UnitedStatesEntries.Any(entry => entry.State == state))
            return Country.UnitedStates;

        // Stryker disable once Equality: see docs/tooling/stryker.md, section "7. AddressStateCatalog GetCountry Exhaustive Mapping"
        if (CanadaEntries.Any(entry => entry.State == state))
            return Country.Canada;

        // Stryker disable once Statement: see docs/tooling/stryker.md, section "7. AddressStateCatalog GetCountry Exhaustive Mapping"
        throw new DomainValidationException(DomainErrors.Address.StateNotInCatalog);
    }

    private static readonly (AddressState State, string DisplayName)[] MexicoEntries =
    [
        (AddressState.Aguascalientes, "Aguascalientes"),
        (AddressState.BajaCalifornia, "Baja California"),
        (AddressState.BajaCaliforniaSur, "Baja California Sur"),
        (AddressState.Campeche, "Campeche"),
        (AddressState.Chiapas, "Chiapas"),
        (AddressState.Chihuahua, "Chihuahua"),
        (AddressState.CiudadDeMexico, "Ciudad de México"),
        (AddressState.Coahuila, "Coahuila"),
        (AddressState.Colima, "Colima"),
        (AddressState.Durango, "Durango"),
        (AddressState.EstadoDeMexico, "Estado de México"),
        (AddressState.Guanajuato, "Guanajuato"),
        (AddressState.Guerrero, "Guerrero"),
        (AddressState.Hidalgo, "Hidalgo"),
        (AddressState.Jalisco, "Jalisco"),
        (AddressState.Michoacan, "Michoacán"),
        (AddressState.Morelos, "Morelos"),
        (AddressState.Nayarit, "Nayarit"),
        (AddressState.NuevoLeon, "Nuevo León"),
        (AddressState.Oaxaca, "Oaxaca"),
        (AddressState.Puebla, "Puebla"),
        (AddressState.Queretaro, "Querétaro"),
        (AddressState.QuintanaRoo, "Quintana Roo"),
        (AddressState.SanLuisPotosi, "San Luis Potosí"),
        (AddressState.Sinaloa, "Sinaloa"),
        (AddressState.Sonora, "Sonora"),
        (AddressState.Tabasco, "Tabasco"),
        (AddressState.Tamaulipas, "Tamaulipas"),
        (AddressState.Tlaxcala, "Tlaxcala"),
        (AddressState.Veracruz, "Veracruz"),
        (AddressState.Yucatan, "Yucatán"),
        (AddressState.Zacatecas, "Zacatecas"),
    ];

    private static readonly (AddressState State, string DisplayName)[] UnitedStatesEntries =
    [
        (AddressState.Alabama, "Alabama"),
        (AddressState.Alaska, "Alaska"),
        (AddressState.Arizona, "Arizona"),
        (AddressState.Arkansas, "Arkansas"),
        (AddressState.California, "California"),
        (AddressState.CarolinaDelNorte, "Carolina del Norte"),
        (AddressState.CarolinaDelSur, "Carolina del Sur"),
        (AddressState.Colorado, "Colorado"),
        (AddressState.Connecticut, "Connecticut"),
        (AddressState.DakotaDelNorte, "Dakota del Norte"),
        (AddressState.DakotaDelSur, "Dakota del Sur"),
        (AddressState.Delaware, "Delaware"),
        (AddressState.Florida, "Florida"),
        (AddressState.Georgia, "Georgia"),
        (AddressState.Hawaii, "Hawái"),
        (AddressState.Idaho, "Idaho"),
        (AddressState.Illinois, "Illinois"),
        (AddressState.Indiana, "Indiana"),
        (AddressState.Iowa, "Iowa"),
        (AddressState.Kansas, "Kansas"),
        (AddressState.Kentucky, "Kentucky"),
        (AddressState.Luisiana, "Luisiana"),
        (AddressState.Maine, "Maine"),
        (AddressState.Maryland, "Maryland"),
        (AddressState.Massachusetts, "Massachusetts"),
        (AddressState.Michigan, "Míchigan"),
        (AddressState.Minnesota, "Minnesota"),
        (AddressState.Misisipi, "Misisipi"),
        (AddressState.Misuri, "Misuri"),
        (AddressState.Montana, "Montana"),
        (AddressState.Nebraska, "Nebraska"),
        (AddressState.Nevada, "Nevada"),
        (AddressState.NuevaJersey, "Nueva Jersey"),
        (AddressState.NuevaYork, "Nueva York"),
        (AddressState.NuevoHampshire, "Nuevo Hampshire"),
        (AddressState.NuevoMexico, "Nuevo México"),
        (AddressState.Ohio, "Ohio"),
        (AddressState.Oklahoma, "Oklahoma"),
        (AddressState.Oregon, "Oregón"),
        (AddressState.Pensilvania, "Pensilvania"),
        (AddressState.RhodeIsland, "Rhode Island"),
        (AddressState.Tennessee, "Tennessee"),
        (AddressState.Texas, "Texas"),
        (AddressState.Utah, "Utah"),
        (AddressState.Vermont, "Vermont"),
        (AddressState.Virginia, "Virginia"),
        (AddressState.VirginiaOccidental, "Virginia Occidental"),
        (AddressState.Washington, "Washington"),
        (AddressState.Wisconsin, "Wisconsin"),
        (AddressState.Wyoming, "Wyoming"),
    ];

    private static readonly (AddressState State, string DisplayName)[] CanadaEntries =
    [
        (AddressState.Alberta, "Alberta"),
        (AddressState.ColumbiaBritanica, "Columbia Británica"),
        (AddressState.IslaDelPrincipeEduardo, "Isla del Príncipe Eduardo"),
        (AddressState.Manitoba, "Manitoba"),
        (AddressState.NuevaEscocia, "Nueva Escocia"),
        (AddressState.NuevoBrunswick, "Nuevo Brunswick"),
        (AddressState.Ontario, "Ontario"),
        (AddressState.Quebec, "Quebec"),
        (AddressState.Saskatchewan, "Saskatchewan"),
        (AddressState.TerranovaYLabrador, "Terranova y Labrador"),
        (AddressState.Nunavut, "Nunavut"),
        (AddressState.TerritoriosDelNoroeste, "Territorios del Noroeste"),
        (AddressState.Yukon, "Yukón"),
    ];
}
