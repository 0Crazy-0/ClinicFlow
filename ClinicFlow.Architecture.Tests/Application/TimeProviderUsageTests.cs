using ArchUnitNET.xUnitV3;
using ClinicFlow.Architecture.Tests.Common;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchUnitArchitecture = ArchUnitNET.Domain.Architecture;

namespace ClinicFlow.Architecture.Tests.Application;

public class TimeProviderUsageTests
{
    private static readonly ArchUnitArchitecture Architecture =
        ArchitectureContext.FullSolutionArchitecture;

    [Fact]
    public void GetUtcNow_ShouldNotBeCalled_ByAnyProductionApplicationType()
    {
        // Arrange
        var forbiddenMethod = MethodMembers()
            .That()
            .AreDeclaredIn(typeof(TimeProvider))
            .And()
            .HaveNameStartingWith(nameof(TimeProvider.GetUtcNow));

        var rule = ArchitectureLayers
            .ApplicationLayer
            .Should()
            .NotCallAny(forbiddenMethod)
            .Because(
                "timeProvider.GetLocalNow().DateTime must always be used for business rules, "
                    + "so dates come from the clinic timezone instead of raw UTC"
            );

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void DateTimeOffsetUtcDateTimeGetter_ShouldNotBeCalled_ByAnyProductionApplicationType()
    {
        // Arrange
        var forbiddenGetter = MethodMembers()
            .That()
            .AreDeclaredIn(typeof(DateTimeOffset))
            .And()
            .HaveNameStartingWith("get_" + nameof(DateTimeOffset.UtcDateTime));

        var rule = ArchitectureLayers
            .ApplicationLayer
            .Should()
            .NotCallAny(forbiddenGetter)
            .Because(
                "the result of timeProvider.GetLocalNow() must only be consumed through "
                    + ".DateTime, so the clinic wall clock value is used without shifting it"
            );

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void DateTimeOffsetLocalDateTimeGetter_ShouldNotBeCalled_ByAnyProductionApplicationType()
    {
        // Arrange
        var forbiddenGetter = MethodMembers()
            .That()
            .AreDeclaredIn(typeof(DateTimeOffset))
            .And()
            .HaveNameStartingWith("get_" + nameof(DateTimeOffset.LocalDateTime));

        var rule = ArchitectureLayers
            .ApplicationLayer
            .Should()
            .NotCallAny(forbiddenGetter)
            .Because(
                "the result of timeProvider.GetLocalNow() must only be consumed through "
                    + ".DateTime, so the clinic wall clock value is used without shifting it"
            );

        // Act & Assert
        rule.Check(Architecture);
    }
}
