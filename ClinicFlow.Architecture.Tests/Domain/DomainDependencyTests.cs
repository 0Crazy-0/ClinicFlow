using ArchUnitNET.xUnitV3;
using ClinicFlow.Architecture.Tests.Common;
using ArchUnitArchitecture = ArchUnitNET.Domain.Architecture;

namespace ClinicFlow.Architecture.Tests.Domain;

public class DomainDependencyTests
{
    private static readonly ArchUnitArchitecture Architecture = ArchitectureContext.ProductionArchitecture;

    [Fact]
    public void DomainLayer_ShouldNotDependOn_ApplicationLayer()
    {
        // Arrange
        var rule = ArchitectureLayers
            .DomainLayer.Should()
            .NotDependOnAny(ArchitectureLayers.ApplicationLayer)
            .Because("the domain layer must be completely decoupled from the application layer");

        // Act & Assert
        rule.Check(Architecture);
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
        rule.Check(Architecture);
    }
}
