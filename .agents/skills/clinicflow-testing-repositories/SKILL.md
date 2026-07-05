---
name: clinicflow-testing-repositories
description: Use this skill alongside clinicflow-testing-base when writing tests for ClinicFlow repository implementations. Covers PostgresFixture lifecycle, Respawner reset, test data seeding helpers, CreateAsync verification, soft-delete testing triad, and Exists/ExistsExcluding patterns.
---

# ClinicFlow Testing Repositories

Specific rules and conventions for testing repository implementations in the Infrastructure layer. These are **integration tests** that run against a real PostgreSQL database via Testcontainers.

## Fixture Lifecycle

Every repository test class uses primary constructor injection of `PostgresFixture`, implements `IAsyncLifetime`, and exposes the `Context` via a property:

```csharp
public class XRepositoryTests(PostgresFixture fixture) : IAsyncLifetime
{
    private readonly XRepository _sut = new(fixture.Context);
    private ApplicationDbContext Context => fixture.Context;

    public async ValueTask InitializeAsync()
    {
        await fixture.Respawner.ResetAsync(fixture.DbConnection);

        fixture.Context.ChangeTracker.Clear();
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }
}
```

`InitializeAsync` always does two things in order: Respawner reset, then `ChangeTracker.Clear()`. This guarantees each test starts with a clean database and no stale tracked entities.

## Constructor with Extra Dependencies

When the repository requires dependencies beyond `ApplicationDbContext` (e.g., `TimeProvider`), use a traditional constructor instead of a primary constructor. The rest of the pattern remains identical:

```csharp
public class AppointmentRepositoryTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private readonly FakeTimeProvider _fakeTime = new();
    private readonly AppointmentRepository _sut;
    private ApplicationDbContext Context => _fixture.Context;

    public AppointmentRepositoryTests(PostgresFixture fixture)
    {
        _fixture = fixture;
        _sut = new AppointmentRepository(fixture.Context, _fakeTime);
    }

    public async ValueTask InitializeAsync()
    {
        await _fixture.Respawner.ResetAsync(_fixture.DbConnection);

        _fixture.Context.ChangeTracker.Clear();
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }
}
```

## Test Data Seeding

Use **`private static` expression-body helpers** for entities without foreign key dependencies:

```csharp
private static AppointmentTypeDefinition CreateAppointmentType(
    string name = "Name",
    AppointmentCategory category = AppointmentCategory.FirstConsultation
) =>
    AppointmentTypeDefinition.Create(
        category,
        name,
        "Description",
        EncounterDuration.FromMinutes(20),
        AgeEligibilityPolicy.NoRestriction
    );
```

When the repository under test requires entities with foreign key relationships (e.g., `Appointment` needs `Doctor`, `Patient`, and `AppointmentTypeDefinition` to exist in DB), use **`private async Task<T>` helpers** that persist the prerequisites:

```csharp
private async Task<User> CreateUserAsync(UserRole role)
{
    var user = User.Create(
        EmailAddress.Create($"{Guid.NewGuid()}@clinic.com"),
        "password",
        PhoneNumber.Create($"+1555{Random.Shared.Next(1000000, 9999999)}"),
        role
    );

    Context.Users.Add(user);

    await Context.SaveChangesAsync();

    return user;
}
```

For tests that repeatedly need a full entity graph, compose a **composite seeder** returning a tuple:

```csharp
private async Task<(
    Doctor Doctor,
    Patient Patient,
    AppointmentTypeDefinition AppointmentType
)> SeedCommonEntitiesAsync()
{
    var doctorUser = await CreateUserAsync(UserRole.Doctor);
    var doctor = await CreateDoctorAsync(doctorUser.Id);
    // ...
    return (doctor, patient, apptType);
}
```

## Persist Before Act

All test data seeded in the Arrange phase **must** be persisted with `Context.SaveChangesAsync()` before the Act phase:

```csharp
// Arrange
var entity = CreateEntity();

Context.EntitySet.Add(entity);

await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

// Act
var result = await _sut.GetByIdAsync(entity.Id, TestContext.Current.CancellationToken);
```

## CreateAsync Verification

When testing a repository's `CreateAsync` method, always follow this sequence: call `_sut.CreateAsync`, then `Context.SaveChangesAsync`, then verify with an `AsNoTracking()` query to bypass EF's cache:

```csharp
[Fact]
public async Task CreateAsync_ShouldAddEntityToContext()
{
    // Arrange
    var entity = CreateEntity();

    // Act
    await _sut.CreateAsync(entity, TestContext.Current.CancellationToken);
    await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert
    var dbResult = await Context
        .EntitySet.AsNoTracking()
        .FirstOrDefaultAsync(
            e => e.Id == entity.Id,
            TestContext.Current.CancellationToken
        );

    dbResult.Should().NotBeNull();
    dbResult.Name.Should().Be(entity.Name);
    dbResult.IsDeleted.Should().BeFalse();
}
```

## Soft-Delete Testing Triad

For every query method that respects the global query filter (`IsDeleted`), always write these three tests as a group:

1. **Happy path** — entity is active:

```csharp
[Fact]
public async Task GetByIdAsync_ShouldReturnEntity_WhenExistsAndActive()
{
    // Arrange
    var entity = CreateEntity();

    Context.EntitySet.Add(entity);

    await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Act
    var result = await _sut.GetByIdAsync(entity.Id, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeNull();
    result.Id.Should().Be(entity.Id);
    result.IsDeleted.Should().BeFalse();
}
```

2. **Filtered out** — entity is soft-deleted:

```csharp
[Fact]
public async Task GetByIdAsync_ShouldReturnNull_WhenSoftDeleted()
{
    // Arrange
    var entity = CreateEntity();

    entity.Deactivate();

    Context.EntitySet.Add(entity);

    await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Act
    var result = await _sut.GetByIdAsync(entity.Id, TestContext.Current.CancellationToken);

    // Assert
    result.Should().BeNull();
}
```

3. **Bypasses filter** — `IncludingDeleted` variant returns soft-deleted entity:

```csharp
[Fact]
public async Task GetByIdIncludingDeletedAsync_ShouldReturnSoftDeletedEntity()
{
    // Arrange
    var entity = CreateEntity();

    entity.Deactivate();

    Context.EntitySet.Add(entity);

    await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Act
    var result = await _sut.GetByIdIncludingDeletedAsync(
        entity.Id,
        TestContext.Current.CancellationToken
    );

    // Assert
    result.Should().NotBeNull();
    result.Id.Should().Be(entity.Id);
    result.IsDeleted.Should().BeTrue();
}
```

A fourth test — `GetByIdAsync_ShouldReturnNull_WhenDoesNotExist` — always accompanies the triad for the non-existent ID case.

## Exists / ExistsExcluding Pattern

For uniqueness-check methods, always write these test pairs:

**ExistsByX:**

```csharp
[Fact]
public async Task ExistsByNameAsync_ShouldReturnTrue_WhenActiveExistsWithName()
{
    // Arrange — seed active entity with specific name
    // Act — call ExistsByNameAsync with that name
    // Assert — result.Should().BeTrue()
}

[Fact]
public async Task ExistsByNameAsync_ShouldReturnFalse_WhenNoActiveWithName()
{
    // Arrange — seed entity, deactivate it
    // Act — call ExistsByNameAsync with that name
    // Assert — result.Should().BeFalse()
}
```

**ExistsByXExcluding** (self-exclusion for updates):

```csharp
[Fact]
public async Task ExistsByNameExcludingAsync_ShouldReturnTrue_WhenAnotherActiveWithNameExists()
{
    // Arrange — seed TWO entities with same name (explain why in comment)
    // Act — call ExistsByNameExcludingAsync excluding entity1.Id
    // Assert — result.Should().BeTrue()
}

[Fact]
public async Task ExistsByNameExcludingAsync_ShouldReturnFalse_WhenOnlySelfMatchesName()
{
    // Arrange — seed ONE entity
    // Act — call ExistsByNameExcludingAsync excluding its own Id
    // Assert — result.Should().BeFalse()
}
```

When seeding duplicate names for the exclusion test, always add a comment explaining that duplicates are prevented at the handler level and are seeded only to test the repository's exclusion logic in isolation:

```csharp
// Duplicate names are prevented at the handler level (see DomainErrors.X.NameAlreadyExists).
// Two are seeded here only to test the repository's exclusion logic in isolation.
```
