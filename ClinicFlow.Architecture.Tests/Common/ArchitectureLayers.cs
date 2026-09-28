using ArchUnitNET.Fluent.Syntax.Elements.Types;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ClinicFlow.Architecture.Tests.Common;

public static class ArchitectureLayers
{
    public static GivenTypesConjunctionWithDescription DomainLayer =>
        Types().That().Are(Layers.DomainTypes).As("Domain Layer");

    public static GivenTypesConjunctionWithDescription DomainTestsLayer =>
        Types().That().Are(Layers.DomainTestsTypes).As("Domain Tests Layer");

    public static GivenTypesConjunctionWithDescription ApplicationLayer =>
        Types().That().Are(Layers.ApplicationTypes).As("Application Layer");

    public static GivenTypesConjunctionWithDescription ApplicationTestsLayer =>
        Types().That().Are(Layers.ApplicationTestsTypes).As("Application Tests Layer");

    public static GivenTypesConjunctionWithDescription InfrastructureLayer =>
        Types().That().Are(Layers.InfrastructureTypes).As("Infrastructure Layer");

    public static GivenTypesConjunctionWithDescription InfrastructureTestsLayer =>
        Types().That().Are(Layers.InfrastructureTestsTypes).As("Infrastructure Tests Layer");
}
