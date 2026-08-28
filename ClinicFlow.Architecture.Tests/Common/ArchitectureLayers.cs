using ArchUnitNET.Fluent.Syntax.Elements.Types;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ClinicFlow.Architecture.Tests.Common;

public static class ArchitectureLayers
{
    public static GivenTypesConjunctionWithDescription DomainLayer =>
        Types().That().ResideInNamespaceMatching(@"^ClinicFlow\.Domain(?:\.|$)").As("Domain Layer");

    public static GivenTypesConjunctionWithDescription DomainTestsLayer =>
        Types()
            .That()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Domain\.Tests(?:\.|$)")
            .As("Domain Tests Layer");

    public static GivenTypesConjunctionWithDescription ApplicationLayer =>
        Types()
            .That()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Application(?:\.|$)")
            .As("Application Layer");

    public static GivenTypesConjunctionWithDescription ApplicationTestsLayer =>
        Types()
            .That()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Application\.Tests(?:\.|$)")
            .As("Application Tests Layer");

    public static GivenTypesConjunctionWithDescription InfrastructureLayer =>
        Types()
            .That()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Infrastructure(?:\.|$)")
            .As("Infrastructure Layer");

    public static GivenTypesConjunctionWithDescription InfrastructureTestsLayer =>
        Types()
            .That()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Infrastructure\.Tests(?:\.|$)")
            .As("Infrastructure Tests Layer");
}
