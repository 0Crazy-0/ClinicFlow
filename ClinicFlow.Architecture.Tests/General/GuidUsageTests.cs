using ArchUnitNET.xUnitV3;
using ClinicFlow.Architecture.Tests.Common;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchUnitArchitecture = ArchUnitNET.Domain.Architecture;

namespace ClinicFlow.Architecture.Tests.General;

public class GuidUsageTests
{
    private static readonly ArchUnitArchitecture Architecture = ArchitectureContext.Architecture;

    [Fact]
    public void GuidNewGuid_ShouldNotBeCalled_ByAnyClass()
    {
        // Arrange
        var forbiddenMethod = MethodMembers()
            .That()
            .AreDeclaredIn(typeof(Guid))
            .And()
            .HaveNameStartingWith(nameof(Guid.NewGuid));

        var rule = Types()
            .That()
            .Are(Layers.ClinicFlowTypes)
            .Should()
            .NotCallAny(forbiddenMethod)
            .Because("Guid.CreateVersion7() must always be used instead of Guid.NewGuid()");

        // Act & Assert
        rule.Check(Architecture);
    }
}
