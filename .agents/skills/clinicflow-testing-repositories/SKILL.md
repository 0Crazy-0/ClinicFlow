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

The standard pattern for repository tests is **`private async Task<T>` helpers** that both create and persist the entity. Each helper adds the entity with `Context.EntitySet.Add(entity)` followed by `await Context.SaveChangesAsync()`. Never use `AddRange`:

```csharp
private async Task<AppointmentTypeDefinition> CreateAppointmentTypeAsync(
    string name = "Name",
    AppointmentCategory category = AppointmentCategory.FirstConsultation
)
{
    var entity = AppointmentTypeDefinition.Create(
        category,
        name,
        "Description",
        EncounterDuration.FromMinutes(20),
        AgeEligibilityPolicy.NoRestriction
    );

    Context.AppointmentTypes.Add(entity);
    await Context.SaveChangesAsync();

    return entity;
}
```

When the entity has foreign key dependencies, chain the prerequisite helpers sequentially (one entity at a time, never `AddRange`):

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

private async Task<Doctor> CreateDoctorAsync(Guid userId)
{
    var specialty = MedicalSpecialty.Create("Cardiology", "Desc", 30, 24);

    Context.MedicalSpecialties.Add(specialty);
    await Context.SaveChangesAsync();

    var doctor = Doctor.Create(
        userId,
        PersonName.Create("Dr. Watson"),
        MedicalLicenseNumber.Create("CMP-" + Guid.NewGuid().ToString("N")[..5]),
        specialty.Id,
        "Desc",
        ConsultationRoom.Create(10, "Room 10", 1)
    );

    Context.Doctors.Add(doctor);
    await Context.SaveChangesAsync();

    return doctor;
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

## CreateAsync / CreateRangeAsync Verification

Use the entity's domain factory method in Arrange (e.g. Create, Schedule, CreateSelf — whatever term the aggregate's ubiquitous language uses), never a private helper. Follow with `_sut.CreateAsync`, then `Context.SaveChangesAsync`, then verify with an `AsNoTracking()` query to bypass EF's cache:

```csharp
[Fact]
public async Task CreateAsync_ShouldAddEntityToContext()
{
    // Arrange
    var entity = Entity.Create(/* params */);

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

    dbResult.Should().BeEquivalentTo(entity);
}
```

For entities with FK dependencies, fetch the prerequisites first using async helpers, then pass them to the domain factory:

```csharp
[Fact]
public async Task CreateAsync_ShouldAddEntityWithPrerequisitesToContext()
{
    // Arrange
    var prerequisite = await CreatePrerequisiteAsync();
    var entity = Entity.Create(prerequisite.Id, /* other params */);

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

    dbResult.Should().BeEquivalentTo(entity);
}
```

When the repository exposes a `CreateRangeAsync` method, verify the inserted collection with `BeEquivalentTo` and a `Where` filter:

```csharp
[Fact]
public async Task CreateRangeAsync_ShouldAddMultipleEntitiesToContext()
{
    // Arrange
    var prerequisite = await CreatePrerequisiteAsync();
    var entity1 = Entity.Create(/* params */);
    var entity2 = Entity.Create(/* params */);

    // Act
    await _sut.CreateRangeAsync([entity1, entity2], TestContext.Current.CancellationToken);
    await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert
    var dbResults = await Context
        .EntitySet.AsNoTracking()
        .Where(e => e.PrerequisiteId == prerequisite.Id)
        .ToListAsync(TestContext.Current.CancellationToken);

    dbResults.Should().BeEquivalentTo([entity1, entity2]);
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
    var entity = await CreateEntityAsync();

    // Act
    var result = await _sut.GetByIdAsync(entity.Id, TestContext.Current.CancellationToken);

    // Assert
    result.Should().BeEquivalentTo(entity);
}
```

2. **Filtered out** — entity is soft-deleted:

```csharp
[Fact]
public async Task GetByIdAsync_ShouldReturnNull_WhenSoftDeleted()
{
    // Arrange
    var entity = await CreateEntityAsync();

    entity.Deactivate();

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
    var entity = await CreateEntityAsync();

    entity.Deactivate();

    await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Act
    var result = await _sut.GetByIdIncludingDeletedAsync(
        entity.Id,
        TestContext.Current.CancellationToken
    );

    // Assert
    result.Should().BeEquivalentTo(entity);
}
```

A fourth test — `GetByIdAsync_ShouldReturnNull_WhenDoesNotExist` — always accompanies the triad for the non-existent ID case:

```csharp
[Fact]
public async Task GetByIdAsync_ShouldReturnNull_WhenDoesNotExist()
{
    // Arrange
    var nonExistentId = Guid.CreateVersion7();

    // Act
    var result = await _sut.GetByIdAsync(nonExistentId, TestContext.Current.CancellationToken);

    // Assert
    result.Should().BeNull();
}
```

## Single-Item Filter Assertion

When a query filters a collection and expects exactly one result matching a condition (e.g., filtering by category or patient while excluding soft-deleted or other-category entities), use the `ContainSingle` + `BeEquivalentTo` chain:

```csharp
[Fact]
public async Task GetByCategoryAsync_ShouldReturnOnlyMatchingActiveCategory()
{
    // Arrange
    var expected = await CreateEntityAsync(category: targetCategory);
    var otherCategoryEntity = await CreateEntityAsync(category: otherCategory);
    var inactive = await CreateEntityAsync(category: targetCategory);

    inactive.Deactivate();

    await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Act
    var results = await _sut.GetByCategoryAsync(targetCategory, TestContext.Current.CancellationToken);

    // Assert
    results.Should().ContainSingle().Which.Should().BeEquivalentTo(expected);
}
```

## Strict Ordering Assertion

When a query applies `OrderBy` or `ThenBy`, assert the expected order explicitly with `WithStrictOrdering()`. Name the test after the actual sort column, not a conceptual one.

### Basic Ascending

```csharp
[Fact]
public async Task GetBySpecialtyIdPaginatedAsync_ShouldReturnDoctorsOrderedByFullNameThenSequenceNumber()
{
    // Arrange
    var entity1 = await CreateEntityAsync(fullName: "Alice");
    var entity2 = await CreateEntityAsync(fullName: "Bob");
    var entity3 = await CreateEntityAsync(fullName: "Charlie");

    // Act
    var (items, totalCount) = await _sut.GetBySpecialtyIdPaginatedAsync(
        specialtyId, 1, 2, TestContext.Current.CancellationToken
    );

    // Assert
    totalCount.Should().Be(3);
    items.Should().BeEquivalentTo([entity1, entity2], options => options.WithStrictOrdering());
}
```

### Descending

When the query uses `OrderByDescending`, seed entities so the collection produces the first entity as the last in insertion order:

```csharp
[Fact]
public async Task GetHistoryByPatientIdAsync_ShouldReturnPenaltiesOrderedBySequenceNumberDescending()
{
    // Arrange
    // SequenceNumber increments auto: entity1=1, entity2=2, entity3=3
    // Descending means entity3 appears first
    var entity1 = await CreateEntityAsync();
    var entity2 = await CreateEntityAsync();
    var entity3 = await CreateEntityAsync();

    // Act
    var results = await _sut.GetHistoryByPatientIdAsync(patientId, TestContext.Current.CancellationToken);

    // Assert
    results.Should().BeEquivalentTo([entity3, entity2, entity1], options => options.WithStrictOrdering());
}
```

### Tiebreaker

When the primary sort column alone cannot guarantee deterministic order (multiple entities share the same value), verify the tiebreaker column independently:

```csharp
[Fact]
public async Task GetByDoctorIdAndDateAsync_ShouldOrderBySequenceNumberAscending_WhenTimeRangeStartIsEqual()
{
    // Arrange
    // These entities share TimeRangeStart=9:00 (primary sort value); SequenceNumber breaks the tie
    var entity1 = await CreateEntityAsync(startHour: 9);  // SequenceNumber=1
    var entity2 = await CreateEntityAsync(startHour: 9);  // SequenceNumber=2
    var entity3 = await CreateEntityAsync(startHour: 9);  // SequenceNumber=3

    // Act
    var results = await _sut.GetByDoctorIdAndDateAsync(doctorId, date, TestContext.Current.CancellationToken);

    // Assert
    results.Should().BeEquivalentTo([entity1, entity2, entity3], options => options.WithStrictOrdering());
}
```

If the entity generates domain events (e.g., `Appointment`), chain the exclusion:

```csharp
results.Should().BeEquivalentTo([entity1, entity2], options => options
    .Excluding(a => a.DomainEvents)
    .WithStrictOrdering());
```

### Unordered Queries

If the query has **no** `OrderBy`/`ThenBy`, omit `WithStrictOrdering()`. Use `BeEquivalentTo([...])` without ordering options.

## Domain Events Exclusion

When creating an entity triggers domain events (e.g., `Appointment.Schedule` adds an `AppointmentScheduledEvent`), the in-memory entity will hold those events while the entity reloaded from the database won't — EF reconstructs it purely from persisted columns, and domain events are never part of that state. Exclude `DomainEvents` from the comparison:

```csharp
[Fact]
public async Task GetByIdAsync_ShouldReturnEntity_WhenExists()
{
    // Arrange
    var entity = await CreateEntityAsync();  // constructor generates domain events

    // Act
    var result = await _sut.GetByIdAsync(entity.Id, TestContext.Current.CancellationToken);

    // Assert
    result.Should().BeEquivalentTo(entity, options => options.Excluding(a => a.DomainEvents));
}
```

Use `.Excluding(a => a.DomainEvents)` **only** when the entity's construction or mutation generates domain events. If the entity type never generates events, the exclusion is unnecessary.

## Exists / ExistsExcluding Pattern

For uniqueness-check methods, always write these test pairs:

**ExistsByNameAsync:**

```csharp
[Fact]
public async Task ExistsByNameAsync_ShouldReturnTrue_WhenActiveExistsWithName()
{
    // Arrange
    var entity = await CreateEntityAsync("TargetName");

    // Act
    var result = await _sut.ExistsByNameAsync("TargetName", TestContext.Current.CancellationToken);

    // Assert
    result.Should().BeTrue();
}

[Fact]
public async Task ExistsByNameAsync_ShouldReturnFalse_WhenNoActiveWithName()
{
    // Arrange
    var entity = await CreateEntityAsync("TargetName");

    entity.Deactivate();

    await Context.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Act
    var result = await _sut.ExistsByNameAsync("TargetName", TestContext.Current.CancellationToken);

    // Assert
    result.Should().BeFalse();
}
```

**ExistsByNameExcludingAsync** (self-exclusion for updates):

```csharp
[Fact]
public async Task ExistsByNameExcludingAsync_ShouldReturnTrue_WhenAnotherActiveWithNameExists()
{
    // Arrange
    // Duplicate names are prevented at the handler level (see DomainErrors.X.NameAlreadyExists).
    // Two are seeded here only to test the repository's exclusion logic in isolation.
    var entity1 = await CreateEntityAsync("SharedName");
    var entity2 = await CreateEntityAsync("SharedName");

    // Act
    var result = await _sut.ExistsByNameExcludingAsync(
        "SharedName",
        entity1.Id,
        TestContext.Current.CancellationToken
    );

    // Assert
    result.Should().BeTrue();
}

[Fact]
public async Task ExistsByNameExcludingAsync_ShouldReturnFalse_WhenOnlySelfMatchesName()
{
    // Arrange
    var entity = await CreateEntityAsync("TargetName");

    // Act
    var result = await _sut.ExistsByNameExcludingAsync(
        "TargetName",
        entity.Id,
        TestContext.Current.CancellationToken
    );

    // Assert
    result.Should().BeFalse();
}
```

When seeding duplicate names for the exclusion test, always add a comment explaining that duplicates are prevented at the handler level and are seeded only to test the repository's exclusion logic in isolation:

```csharp
// Duplicate names are prevented at the handler level (see DomainErrors.X.NameAlreadyExists).
// Two are seeded here only to test the repository's exclusion logic in isolation.
```
