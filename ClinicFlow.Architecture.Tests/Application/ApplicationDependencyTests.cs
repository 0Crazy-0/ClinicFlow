using ArchUnitNET.xUnitV3;
using ClinicFlow.Architecture.Tests.Common;
using ArchUnitArchitecture = ArchUnitNET.Domain.Architecture;

namespace ClinicFlow.Architecture.Tests.Application;

public class ApplicationDependencyTests
{
    private static readonly ArchUnitArchitecture ProductionArchitecture =
        ArchitectureContext.ProductionArchitecture;
    private static readonly ArchUnitArchitecture TestsArchitecture =
        ArchitectureContext.TestsArchitecture;

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
        rule.Check(ProductionArchitecture);
    }

    [Fact]
    public void ApplicationTestsLayer_ShouldNotDependOn_InfrastructureTestsLayer()
    {
        // Arrange
        var rule = ArchitectureLayers
            .ApplicationTestsLayer.Should()
            .NotDependOnAny(ArchitectureLayers.InfrastructureTestsLayer)
            .Because(
                "the application tests layer must be completely decoupled from the infrastructure tests layer"
            );

        // Act & Assert
        rule.Check(TestsArchitecture);
    }
}
