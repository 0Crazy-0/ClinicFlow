using ArchUnitNET.xUnitV3;
using ClinicFlow.Architecture.Tests.Common;
using ClinicFlow.Infrastructure.Persistence;
using ClinicFlow.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchUnitArchitecture = ArchUnitNET.Domain.Architecture;

namespace ClinicFlow.Architecture.Tests.Infrastructure;

public class InfrastructureSealedConventionTests
{
    private static readonly ArchUnitArchitecture Architecture =
        ArchitectureContext.ProductionArchitecture;

    [Fact]
    public void Repositories_ShouldBeSealed()
    {
        // Arrange
        var repositoryInterfaces = Types()
            .That()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Domain\.Interfaces\.Repositories(?:\.|$)")
            .As("repository interfaces");

        var rule = Classes()
            .That()
            .AreAssignableTo(repositoryInterfaces)
            .And()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Infrastructure(?:\.|$)")
            .Should()
            .BeSealed()
            .Because(
                "repository implementations are leaf classes that only translate domain "
                    + "contracts into EF Core queries, so they must be sealed"
            );

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void UnitOfWork_ShouldBeSealed()
    {
        // Arrange
        var rule = Classes()
            .That()
            .HaveFullName(typeof(UnitOfWork).FullName)
            .Should()
            .BeSealed()
            .Because(
                "the unit of work is a leaf class that only coordinates persistence and "
                    + "event publication, so it must be sealed"
            );

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void ClinicTimeProvider_ShouldBeSealed()
    {
        // Arrange
        var rule = Classes()
            .That()
            .HaveFullName(typeof(ClinicTimeProvider).FullName)
            .Should()
            .BeSealed()
            .Because(
                "the clinic time provider is a leaf class that only adapts time reading "
                    + "to the clinic timezone, so it must be sealed"
            );

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void EntityConfigurations_ShouldBeSealed()
    {
        // Arrange
        var rule = Classes()
            .That()
            .AreAssignableTo(typeof(IEntityTypeConfiguration<>))
            .And()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Infrastructure(?:\.|$)")
            .Should()
            .BeSealed()
            .Because(
                "entity type configurations are leaf classes that only apply EF Core "
                    + "mappings, so they must be sealed"
            );

        // Act & Assert
        rule.Check(Architecture);
    }
}
