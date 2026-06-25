using ClinicFlow.Infrastructure.Tests.Shared;

namespace ClinicFlow.Infrastructure.Tests.Collections;

[CollectionDefinition("PostgresTests")]
public class PostgresTestsCollection : ICollectionFixture<DbSeederFixture> { }
