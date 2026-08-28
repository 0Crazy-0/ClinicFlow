using ArchUnitNET.xUnitV3;
using ClinicFlow.Architecture.Tests.Common;
using ArchUnitArchitecture = ArchUnitNET.Domain.Architecture;

namespace ClinicFlow.Architecture.Tests.Domain;

public class DomainDependencyTests
{
    private static readonly ArchUnitArchitecture ProductionArchitecture =
        ArchitectureContext.ProductionArchitecture;
    private static readonly ArchUnitArchitecture TestsArchitecture =
        ArchitectureContext.TestsArchitecture;

    [Fact]
    public void DomainLayer_ShouldNotDependOn_ApplicationLayer()
    {
        // Arrange
        var rule = ArchitectureLayers
            .DomainLayer.Should()
            .NotDependOnAny(ArchitectureLayers.ApplicationLayer)
            .Because("the domain layer must be completely decoupled from the application layer");

        // Act & Assert
        rule.Check(ProductionArchitecture);
    }

    [Fact]
    public void DomainLayer_ShouldNotDependOn_InfrastructureLayer()
    {
        // Arrange
        var rule = ArchitectureLayers
            .DomainLayer.Should()
            .NotDependOnAny(ArchitectureLayers.InfrastructureLayer)
            .Because("the domain layer must be completely decoupled from the infrastructure layer");

        // Act & Assert
        rule.Check(ProductionArchitecture);
    }

    [Fact]
    public void DomainTestsLayer_ShouldNotDependOn_ApplicationTestsLayer()
    {
        // Arrange
        var rule = ArchitectureLayers
            .DomainTestsLayer.Should()
            .NotDependOnAny(ArchitectureLayers.ApplicationTestsLayer)
            .Because(
                "the domain tests layer must be completely decoupled from the application tests layer"
            );

        // Act & Assert
        rule.Check(TestsArchitecture);
    }

    [Fact]
    public void DomainTestsLayer_ShouldNotDependOn_InfrastructureTestsLayer()
    {
        // Arrange
        var rule = ArchitectureLayers
            .DomainTestsLayer.Should()
            .NotDependOnAny(ArchitectureLayers.InfrastructureTestsLayer)
            .Because(
                "the domain tests layer must be completely decoupled from the infrastructure tests layer"
            );

        // Act & Assert
        rule.Check(TestsArchitecture);
    }
}
