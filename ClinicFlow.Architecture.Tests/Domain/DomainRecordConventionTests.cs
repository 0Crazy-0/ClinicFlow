using ArchUnitNET.xUnitV3;
using ClinicFlow.Architecture.Tests.Common;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchUnitArchitecture = ArchUnitNET.Domain.Architecture;

namespace ClinicFlow.Architecture.Tests.Domain;

public class DomainRecordConventionTests
{
    private static readonly ArchUnitArchitecture Architecture =
        ArchitectureContext.ProductionArchitecture;

    [Fact]
    public void DomainContexts_ShouldBeRecords()
    {
        // Arrange
        var rule = Classes()
            .That()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Domain\.Services\.Contexts(?:\.|$)")
            .Should()
            .BeRecord()
            .Because(
                "contexts are immutable data carriers of pre-fetched world state, "
                    + "so they must be records"
            );

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void DomainArgs_ShouldBeRecords()
    {
        // Arrange
        var rule = Classes()
            .That()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Domain\.Services\.Args(?:\.|$)")
            .Should()
            .BeRecord()
            .Because("args are immutable data carriers of actor intent, so they must be records");

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void DomainEvents_ShouldBeRecords()
    {
        // Arrange
        var rule = Classes()
            .That()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Domain\.Events(?:\.|$)")
            .Should()
            .BeRecord()
            .Because(
                "domain events are immutable facts about the past with value semantics, "
                    + "so they must be records"
            );

        // Act & Assert
        rule.Check(Architecture);
    }
}
