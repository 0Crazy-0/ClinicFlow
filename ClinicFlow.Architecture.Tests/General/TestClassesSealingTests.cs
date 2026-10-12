using ArchUnitNET.xUnitV3;
using ClinicFlow.Architecture.Tests.Common;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchUnitArchitecture = ArchUnitNET.Domain.Architecture;

namespace ClinicFlow.Architecture.Tests.General;

public class TestClassesSealingTests
{
    private static readonly ArchUnitArchitecture Architecture =
        ArchitectureContext.FullSolutionArchitecture;

    [Fact]
    public void TestClasses_ShouldNotBeSealed_InAnyLayer()
    {
        var rule = MethodMembers()
            .That()
            .HaveAnyAttributes(typeof(FactAttribute), typeof(TheoryAttribute))
            .Should()
            .BeDeclaredIn(Classes().That().AreNotSealed())
            .Because(
                "sealed is reserved for well-justified production classes only, so no "
                    + "test class in any layer may be sealed"
            );

        rule.Check(Architecture);
    }
}
