using ArchUnitNET.xUnitV3;
using ClinicFlow.Architecture.Tests.Common;
using ArchUnitArchitecture = ArchUnitNET.Domain.Architecture;

namespace ClinicFlow.Architecture.Tests.Application;

public class ApplicationDependencyTests
{
    private static readonly ArchUnitArchitecture Architecture = ArchitectureContext.ProductionArchitecture;

    [Fact]
    public void ApplicationLayer_ShouldNotDependOn_InfrastructureLayer()
    {
        // Arrange
        var rule = ArchitectureLayers
            .ApplicationLayer.Should()
            .NotDependOnAny(ArchitectureLayers.InfrastructureLayer)
            .Because(
                "the application layer must be completely decoupled from the infrastructure layer"
            );

        // Act & Assert
        rule.Check(Architecture);
    }
}
