using AwesomeAssertions;

namespace ClinicFlow.Architecture.Tests.Common;

public class ArchitectureContextTests
{
    [Fact]
    public void FullSolutionArchitecture_ShouldLoad_AllExpectedAssemblies() =>
        ArchitectureContext.FullSolutionArchitecture.Should().NotBeNull();

    [Fact]
    public void ProductionArchitecture_ShouldLoad_AllExpectedAssemblies() =>
        ArchitectureContext.ProductionArchitecture.Should().NotBeNull();
}
