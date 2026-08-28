using System.Reflection;
using ArchUnitNET.Loader;
using ArchUnitArchitecture = ArchUnitNET.Domain.Architecture;

namespace ClinicFlow.Architecture.Tests.Common;

public static class ArchitectureContext
{
    public static readonly ArchUnitArchitecture FullSolutionArchitecture = new ArchLoader()
        .LoadAssemblies(
            Assembly.Load("ClinicFlow.Domain"),
            Assembly.Load("ClinicFlow.Domain.Tests"),
            Assembly.Load("ClinicFlow.Application"),
            Assembly.Load("ClinicFlow.Application.Tests"),
            Assembly.Load("ClinicFlow.Infrastructure"),
            Assembly.Load("ClinicFlow.Infrastructure.Tests"),
            Assembly.Load("ClinicFlow.Architecture.Tests"),
            typeof(Guid).Assembly
        )
        .Build();

    public static readonly ArchUnitArchitecture ProductionArchitecture = new ArchLoader()
        .LoadAssemblies(
            Assembly.Load("ClinicFlow.Domain"),
            Assembly.Load("ClinicFlow.Application"),
            Assembly.Load("ClinicFlow.Infrastructure")
        )
        .Build();
}
