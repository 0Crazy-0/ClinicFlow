using ArchUnitNET.xUnitV3;
using ClinicFlow.Architecture.Tests.Common;
using MediatR;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchUnitArchitecture = ArchUnitNET.Domain.Architecture;

namespace ClinicFlow.Architecture.Tests.Application;

public class ApplicationRecordConventionTests
{
    private static readonly ArchUnitArchitecture Architecture =
        ArchitectureContext.ProductionArchitecture;

    [Fact]
    public void CommandsAndQueries_ShouldBeRecords()
    {
        // Arrange
        var rule = Classes()
            .That()
            .AreAssignableTo(typeof(IBaseRequest))
            .And()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Application(?:\.|$)")
            .Should()
            .BeRecord()
            .Because(
                "commands and queries are immutable MediatR messages with value semantics, "
                    + "so they must be records"
            );

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void Dtos_ShouldBeRecords()
    {
        // Arrange
        var rule = Classes()
            .That()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Application(?:\.|$)")
            .And()
            .HaveNameEndingWith("Dto")
            .Should()
            .BeRecord()
            .Because(
                "query result DTOs are immutable data carriers with value semantics, "
                    + "so they must be records"
            );

        // Act & Assert
        rule.Check(Architecture);
    }
}
