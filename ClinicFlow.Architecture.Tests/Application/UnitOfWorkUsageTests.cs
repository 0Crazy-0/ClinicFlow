using ArchUnitNET.xUnitV3;
using ClinicFlow.Architecture.Tests.Common;
using ClinicFlow.Domain.Interfaces;
using ClinicFlow.Infrastructure;
using ClinicFlow.Infrastructure.Persistence;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchUnitArchitecture = ArchUnitNET.Domain.Architecture;

namespace ClinicFlow.Architecture.Tests.Application;

public class UnitOfWorkUsageTests
{
    private static readonly ArchUnitArchitecture ProductionArchitecture =
        ArchitectureContext.ProductionArchitecture;

    [Fact]
    public void OnlyCommandHandlersAndEventHandlers_ShouldDependOn_IUnitOfWork()
    {
        // Arrange
        var unitOfWork = Types()
            .That()
            .HaveFullName(typeof(IUnitOfWork).FullName)
            .As("IUnitOfWork");

        var rule = Types()
            .That()
            .Are(Layers.ApplicationTypes)
            .And()
            .DependOnAny(unitOfWork)
            .Should()
            .HaveNameEndingWith("CommandHandler")
            .OrShould()
            .HaveNameEndingWith("EventHandler")
            .Because(
                "the unit of work must only be injected in command handlers and event handlers, "
                    + "so query handlers stay read only and never commit changes"
            );

        // Act & Assert
        rule.Check(ProductionArchitecture);
    }

    [Fact]
    public void DomainLayer_ShouldNotDependOn_IUnitOfWork()
    {
        // Arrange
        var unitOfWork = Types()
            .That()
            .HaveFullName(typeof(IUnitOfWork).FullName)
            .As("IUnitOfWork");

        var rule = ArchitectureLayers
            .DomainLayer.Should()
            .NotDependOnAny(unitOfWork)
            .Because(
                "only the application production layer may depend on the unit of work, "
                    + "so the domain stays decoupled from the transaction boundary"
            );

        // Act & Assert
        rule.Check(ProductionArchitecture);
    }

    [Fact]
    public void InfrastructureLayer_ShouldNotDependOn_IUnitOfWork()
    {
        // Arrange
        var unitOfWork = Types()
            .That()
            .HaveFullName(typeof(IUnitOfWork).FullName)
            .As("IUnitOfWork");

        var rule = Types()
            .That()
            .Are(Layers.InfrastructureTypes)
            .And()
            .DoNotHaveFullName(typeof(UnitOfWork).FullName)
            .And()
            .DoNotHaveFullName(typeof(DependencyInjection).FullName)
            .Should()
            .NotDependOnAny(unitOfWork)
            .Because(
                "only the application production layer may depend on the unit of work, "
                    + "so infrastructure only implements and registers it without consuming it"
            );

        // Act & Assert
        rule.Check(ProductionArchitecture);
    }
}
