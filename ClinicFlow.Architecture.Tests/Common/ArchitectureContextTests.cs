using AwesomeAssertions;

namespace ClinicFlow.Architecture.Tests.Common;

public class ArchitectureContextTests
{
    [Fact]
    public void FullSolutionArchitecture_ShouldLoad_AllExpectedAssemblies() =>
        ArchitectureContext.FullSolutionArchitecture.Should().NotBeNull();

    [Fact]
    public void ProductionArchitecture_ShouldLoad_AllExpectedAssemblies()
    {
        ArchitectureContext.ProductionArchitecture.Should().NotBeNull();

        Layers
            .DomainTypes.GetObjects(ArchitectureContext.ProductionArchitecture)
            .Should()
            .NotBeEmpty();
        Layers
            .ApplicationTypes.GetObjects(ArchitectureContext.ProductionArchitecture)
            .Should()
            .NotBeEmpty();
        Layers
            .InfrastructureTypes.GetObjects(ArchitectureContext.ProductionArchitecture)
            .Should()
            .NotBeEmpty();
    }

    [Fact]
    public void TestsArchitecture_ShouldLoad_AllExpectedAssemblies()
    {
        ArchitectureContext.TestsArchitecture.Should().NotBeNull();

        Layers
            .DomainTestsTypes.GetObjects(ArchitectureContext.TestsArchitecture)
            .Should()
            .NotBeEmpty();
        Layers
            .ApplicationTestsTypes.GetObjects(ArchitectureContext.TestsArchitecture)
            .Should()
            .NotBeEmpty();
        Layers
            .InfrastructureTestsTypes.GetObjects(ArchitectureContext.TestsArchitecture)
            .Should()
            .NotBeEmpty();
    }
}
