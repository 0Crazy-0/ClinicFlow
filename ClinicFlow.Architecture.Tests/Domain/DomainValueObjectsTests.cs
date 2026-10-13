using ArchUnitNET.xUnitV3;
using ClinicFlow.Architecture.Tests.Common;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchUnitArchitecture = ArchUnitNET.Domain.Architecture;

namespace ClinicFlow.Architecture.Tests.Domain;

public class DomainValueObjectsTests
{
    private static readonly ArchUnitArchitecture Architecture =
        ArchitectureContext.ProductionArchitecture;

    [Fact]
    public void DomainValueObjects_ShouldBeRecords()
    {
        // Arrange
        var rule = Classes()
            .That()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Domain\.ValueObjects(?:\.|$)")
            .Should()
            .BeRecord()
            .Because(
                "value objects are immutable data carriers with value semantics, "
                    + "so they must be records"
            );

        // Act & Assert
        rule.Check(Architecture);
    }
}
