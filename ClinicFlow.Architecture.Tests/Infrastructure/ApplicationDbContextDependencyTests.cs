using ArchUnitNET.xUnitV3;
using ClinicFlow.Architecture.Tests.Common;
using ClinicFlow.Infrastructure;
using ClinicFlow.Infrastructure.Persistence;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchUnitArchitecture = ArchUnitNET.Domain.Architecture;

namespace ClinicFlow.Architecture.Tests.Infrastructure;

public class ApplicationDbContextDependencyTests
{
    private static readonly ArchUnitArchitecture Architecture =
        ArchitectureContext.ProductionArchitecture;

    [Fact]
    public void OnlyRepositoriesAndUnitOfWork_ShouldDependOn_ApplicationDbContext()
    {
        // Arrange
        var dbContext = Types()
            .That()
            .HaveFullName(typeof(ApplicationDbContext).FullName)
            .As("ApplicationDbContext");

        var rule = Types()
            .That()
            .Are(Layers.InfrastructureTypes)
            .And()
            .DependOnAny(dbContext)
            .And()
            .DoNotResideInNamespaceMatching(@"^ClinicFlow\.Infrastructure\.Migrations(?:\.|$)")
            .And()
            .DoNotResideInNamespaceMatching(
                @"^ClinicFlow\.Infrastructure\.Persistence\.Seeding(?:\.|$)"
            )
            .And()
            .DoNotHaveFullName(typeof(ApplicationDbContext).FullName)
            .And()
            .DoNotHaveFullName(typeof(DependencyInjection).FullName)
            .And()
            .DoNotHaveFullName(typeof(ApplicationDbContextFactory).FullName)
            .Should()
            .ResideInNamespaceMatching(
                @"^ClinicFlow\.Infrastructure\.Persistence\.Repositories(?:\.|$)"
            )
            .OrShould()
            .HaveFullName(typeof(UnitOfWork).FullName)
            .Because(
                "only repositories and the unit of work may take ApplicationDbContext, "
                    + "so persistence access stays behind those abstractions"
            );

        // Act & Assert
        rule.Check(Architecture);
    }
}
