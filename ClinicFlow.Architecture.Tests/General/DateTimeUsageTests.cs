using ArchUnitNET.xUnitV3;
using ClinicFlow.Architecture.Tests.Common;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchUnitArchitecture = ArchUnitNET.Domain.Architecture;

namespace ClinicFlow.Architecture.Tests.General;

public class DateTimeUsageTests
{
    private static readonly ArchUnitArchitecture Architecture =
        ArchitectureContext.FullSolutionArchitecture;

    [Fact]
    public void DateTimeUtcNow_ShouldNotBeCalled_ByAnyClass()
    {
        // Arrange
        var forbiddenMethod = MethodMembers()
            .That()
            .AreDeclaredIn(typeof(DateTime))
            .And()
            .HaveNameStartingWith("get_" + nameof(DateTime.UtcNow));

        var rule = Types()
            .That()
            .Are(Layers.ClinicFlowTypes)
            .Should()
            .NotCallAny(forbiddenMethod)
            .Because(
                "TimeProvider must always be used instead of DateTime.UtcNow to remain decoupled from the real-world clock"
            );

        // Act & Assert
        rule.Check(Architecture);
    }
}
