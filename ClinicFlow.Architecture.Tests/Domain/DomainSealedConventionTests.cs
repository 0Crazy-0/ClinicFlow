using ArchUnitNET.xUnitV3;
using ClinicFlow.Architecture.Tests.Common;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchUnitArchitecture = ArchUnitNET.Domain.Architecture;

namespace ClinicFlow.Architecture.Tests.Domain;

public class DomainSealedConventionTests
{
    private static readonly ArchUnitArchitecture Architecture =
        ArchitectureContext.ProductionArchitecture;

    [Fact]
    public void DomainContexts_ShouldBeSealed()
    {
        // Arrange
        var rule = Classes()
            .That()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Domain\.Services\.Contexts(?:\.|$)")
            .Should()
            .BeSealed()
            .Because(
                "contexts are immutable data carriers of pre-fetched world state, "
                    + "so they must be sealed"
            );

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void DomainArgs_ShouldBeSealed()
    {
        // Arrange
        var rule = Classes()
            .That()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Domain\.Services\.Args(?:\.|$)")
            .Should()
            .BeSealed()
            .Because("args are immutable data carriers of actor intent, so they must be sealed");

        // Act & Assert
        rule.Check(Architecture);
    }
}
