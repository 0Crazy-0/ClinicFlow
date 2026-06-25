using ClinicFlow.Infrastructure.Tests.Shared;

// Disable parallel execution of tests within this assembly because they share a static PostgreSQL Testcontainer
// and use Bogus with a static seed (Randomizer.Seed), which are not thread-safe.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

// Registers a single shared PostgreSQL Testcontainer fixture for the entire test assembly.
// All test classes receive the same DbSeederFixture instance via constructor injection,
// avoiding the overhead of spinning up multiple containers per test class.
[assembly: AssemblyFixture(typeof(DbSeederFixture))]
