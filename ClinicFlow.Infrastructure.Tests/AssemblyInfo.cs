// Disable parallel execution of tests within this assembly because they share a static PostgreSQL Testcontainer
// and use Bogus with a static seed (Randomizer.Seed), which are not thread-safe.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
