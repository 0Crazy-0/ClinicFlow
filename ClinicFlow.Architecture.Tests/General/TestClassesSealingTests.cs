using ArchUnitNET.Domain;
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
        var rule = Classes()
            .That()
            .FollowCustomPredicate(
                static cls =>
                    cls.MembersIncludingInherited.OfType<MethodMember>()
                        .Any(static m =>
                            m.Attributes.Any(static a =>
                                a.FullName == typeof(FactAttribute).FullName
                                || a.FullName == typeof(TheoryAttribute).FullName
                            )
                        ),
                "have test methods (including inherited)"
            )
            .Should()
            .NotBeSealed()
            .Because(
                "sealed is reserved for well-justified production classes only, so no "
                    + "test class in any layer may be sealed"
            );

        rule.Check(Architecture);
    }
}
