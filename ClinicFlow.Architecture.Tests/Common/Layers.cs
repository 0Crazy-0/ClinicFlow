using ArchUnitNET.Domain;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ClinicFlow.Architecture.Tests.Common;

public static class Layers
{
    public static readonly IObjectProvider<IType> ClinicFlowTypes = Types()
        .That()
        .ResideInNamespaceMatching("^ClinicFlow")
        .As("ClinicFlow Types");

    public static readonly IObjectProvider<IType> DomainTypes = Types()
        .That()
        .ResideInNamespaceMatching("^ClinicFlow.Domain")
        .As("Domain Layer");

    public static readonly IObjectProvider<IType> ApplicationTypes = Types()
        .That()
        .ResideInNamespaceMatching("^ClinicFlow.Application")
        .As("Application Layer");

    public static readonly IObjectProvider<IType> InfrastructureTypes = Types()
        .That()
        .ResideInNamespaceMatching("^ClinicFlow.Infrastructure")
        .As("Infrastructure Layer");
}
