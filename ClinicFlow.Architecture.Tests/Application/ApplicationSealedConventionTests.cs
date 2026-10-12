using ArchUnitNET.xUnitV3;
using ClinicFlow.Architecture.Tests.Common;
using FluentValidation;
using MediatR;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchUnitArchitecture = ArchUnitNET.Domain.Architecture;

namespace ClinicFlow.Architecture.Tests.Application;

public class ApplicationSealedConventionTests
{
    private static readonly ArchUnitArchitecture Architecture =
        ArchitectureContext.ProductionArchitecture;

    [Fact]
    public void CommandsAndQueries_ShouldBeSealed()
    {
        // Arrange
        var rule = Classes()
            .That()
            .AreAssignableTo(typeof(IBaseRequest))
            .And()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Application(?:\.|$)")
            .Should()
            .BeSealed()
            .Because("commands and queries are immutable MediatR messages, so they must be sealed");

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void Dtos_ShouldBeSealed()
    {
        // Arrange
        var rule = Classes()
            .That()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Application(?:\.|$)")
            .And()
            .HaveNameEndingWith("Dto")
            .Should()
            .BeSealed()
            .Because("query result DTOs are immutable data carriers, so they must be sealed");

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void Validators_ShouldBeSealed()
    {
        // Arrange
        var rule = Classes()
            .That()
            .AreAssignableTo(typeof(AbstractValidator<>))
            .And()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Application(?:\.|$)")
            .Should()
            .BeSealed()
            .Because(
                "validators are leaf classes, so they must be sealed to prevent "
                    + "accidental subclassing"
            );

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void EventHandlers_ShouldBeSealed()
    {
        // Arrange
        var rule = Classes()
            .That()
            .AreAssignableTo(typeof(INotificationHandler<>))
            .And()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Application(?:\.|$)")
            .Should()
            .BeSealed()
            .Because(
                "domain event handlers are leaf classes, so they must be sealed to "
                    + "prevent accidental subclassing"
            );

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void CommandHandlers_ShouldBeSealed()
    {
        // Arrange
        var rule = Classes()
            .That()
            .AreAssignableTo(typeof(IRequestHandler<>))
            .And()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Application(?:\.|$)")
            .Should()
            .BeSealed()
            .Because(
                "command handlers are leaf classes resolved by MediatR, so they must "
                    + "be sealed to prevent accidental subclassing"
            );

        // Act & Assert
        rule.Check(Architecture);
    }

    [Fact]
    public void QueryHandlers_ShouldBeSealed()
    {
        // Arrange
        var rule = Classes()
            .That()
            .AreAssignableTo(typeof(IRequestHandler<,>))
            .And()
            .ResideInNamespaceMatching(@"^ClinicFlow\.Application(?:\.|$)")
            .Should()
            .BeSealed()
            .Because(
                "query handlers are leaf classes resolved by MediatR, so they must "
                    + "be sealed to prevent accidental subclassing"
            );

        // Act & Assert
        rule.Check(Architecture);
    }
}
