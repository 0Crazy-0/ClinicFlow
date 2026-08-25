# Stryker Guidelines and Conventions

This document outlines the design decisions, conventions, and guidelines for using **Stryker.NET** (mutation testing) within the ClinicFlow project.

Configuration is managed through per-project `stryker-config.json` files:
- [Domain Tests Config](../../ClinicFlow.Domain.Tests/stryker-config.json)
- [Application Tests Config](../../ClinicFlow.Application.Tests/stryker-config.json)
- [Infrastructure Tests Config](../../ClinicFlow.Infrastructure.Tests/stryker-config.json)

For the CI workflow integration, see [.github/workflows/mutation-testing.yml](../../.github/workflows/mutation-testing.yml).

---

## Design Decisions & Philosophy

### 1. Pure Code Philosophy Across All Layers
In alignment with our [Codecov](./codecov.md) and [SonarCloud](./sonarcloud.md) conventions, **we do not use C# attributes (such as `[ExcludeFromCodeCoverage]`) or Stryker suppression comments (such as `// Stryker disable all`, `// Stryker disable once all`, `// Stryker disable once <mutators>`, or `// Stryker restore all`)** anywhere in the Domain, Application, or Infrastructure layers. 

The codebase must speak its own language and remain completely free from tooling-specific annotations and comments. We prioritize code purity, domain clarity, and maintainability over artificially achieving a 100% mutation score by polluting the source code.

### 2. Non-Blocking CI Status & Baseline Drift
Stryker mutation testing runs in CI via GitHub Actions, but **it is intentionally configured as a non-blocking check for Pull Requests**. 

This decision was made due to recurring false positives and tooling limitations in incremental baseline mode:

1. **Pure Configuration Lines in `ApplicationDbContext`:**
   Mutants surviving in `OnModelCreating` fall into two distinct categories:
   - **`base.OnModelCreating(modelBuilder)` statement removal:** EF Core's base `DbContext.OnModelCreating` is an empty no-op, so removing this call produces zero side effects and leaves the model in an identical state — an **equivalent mutant** that cannot be distinguished by any test.
   - **`modelBuilder.HasPostgresExtension("btree_gist")` registration:** This single line registers a PostgreSQL database extension and has no observable branching logic to verify in isolated unit or repository tests; mutants on its string literal are false positives.
   
   This exception is strictly limited to the two lines above. It does **not** extend to the `foreach` loops configuring `SequenceNumber`/`Version` (`BaseEntity`) or the dynamically-built soft-delete query filter (`SoftDeletableEntity`) — both are exercised by `ApplicationDbContextModelTests` and `ApplicationDbContextIntegrationTests` respectively, and any survived mutants there represent real gaps.

2. **Baseline Reset on Accumulated Edits:**
   Stryker's `--with-baseline` mode operates by only evaluating mutants within changed code and comparing against a base commit. When a file accumulates modifications across multiple commits or refactorings, Stryker resets the baseline status for all mutants in the modified blocks. Consequently, previously killed or benign configuration mutants can lose their baseline status and resurface as survived mutants in CI reports without any real regression in business logic.

Due to these false positives and baseline inconsistencies, Stryker is not a required gating check in PRs. Instead, developers verify mutation results locally before pushing. Local runs execute a full (non-baseline) analysis, so new mutants are not automatically isolated from pre-existing ones. Developers should review any survived mutants and confirm they fall into a known false-positive category (see above) rather than representing a real gap in business logic before merging.

### 3. Mutation Score Thresholds & Current Status
Each project defines three mutation score thresholds in its `stryker-config.json`:

| Project | High (Target) | Low (Warning) | Break (Failure) | Current Status |
|---|---|---|---|---|
| **Domain** | ≥ 95% | 90% | < 85% | **100% (0 surviving mutants)** |
| **Application** | ≥ 95% | 90% | < 80% | **100% (0 surviving mutants)** |
| **Infrastructure** | ≥ 95% | 90% | < 85% | [Documented exceptions only](#documented-survived--equivalent-mutants) |

Both **Domain** and **Application** layers achieve and maintain a **100% mutation score with zero surviving mutants**. Every mutation generated across domain entities, value objects, domain services, CQRS command/query handlers, and validators is killed by their corresponding test suites.

The **Infrastructure** layer similarly kills all mutations across repository implementations, persistence logic, and policies, with surviving mutants strictly confined to the four documented exceptions below (repository `CreateRangeAsync` boundary check, `ApplicationDbContext` model registration, `UnitOfWork` events filter, and `ToStableLong` bitwise helper). Outside of these documented cases, Infrastructure is expected to remain completely free of surviving mutants.

---

## Configured Exclusions

Certain file patterns and namespaces are globally excluded from mutation analysis in each `stryker-config.json` because they represent pure data contracts, DI registrations, or boilerplate infrastructure without testable branching logic.

### Domain Layer (`ClinicFlow.Domain.Tests/stryker-config.json`)
- **`**/Enums/**`:** Enum declarations.
- **`**/Interfaces/**`:** Interface definitions.
- **`**/Events/**`:** Simple data-carrying domain event records.
- **`**/Properties/**`:** Assembly-level attributes and metadata (`AssemblyInfo.cs`).
- **`**/Services/Args/**`:** Method argument encapsulation records.
- **`**/Services/Contexts/**`:** Domain service validation context records.
- **`**/Common/IDomainEvent.cs`:** Domain event marker interface.

### Application Layer (`ClinicFlow.Application.Tests/stryker-config.json`)
- **`**/DependencyInjection.cs`:** Service collection registration boilerplate.
- **`**/ValidationException.cs`:** Exception model wrapper.
- **`**/DTOs/**`:** Data transfer objects with no behavior.
- **`**/*Command.cs` & `**/*Query.cs`:** CQRS message definitions (records with primary constructors).

### Infrastructure Layer (`ClinicFlow.Infrastructure.Tests/stryker-config.json`)
- **`**/DependencyInjection.cs`:** Infrastructure service registrations.
- **`**/Migrations/**`:** EF Core auto-generated migration history.
- **`**/Configurations/**`:** EF Core Fluent API entity configurations.
- **`**/ApplicationDbContextFactory.cs`:** Design-time factory for EF Core CLI.
- **`**/ColumnNames.cs`:** Database column name constants.
- **`**/Options/**`:** Configuration binding option classes.
- **`**/Seeding/**`:** Database seed data generators.

---

## Documented Survived & Equivalent Mutants

> [!NOTE]
> **Zero Survived Mutants in Domain and Application:**
> The `ClinicFlow.Domain` and `ClinicFlow.Application` layers have **zero surviving mutants** (100% mutation score).
>
> **Infrastructure Baseline & Zero-Mutant Expectation:**
> The survived and equivalent mutants documented below occur **exclusively within the Infrastructure layer** (`ClinicFlow.Infrastructure`). Outside of these documented exceptions, the Infrastructure layer is expected to be completely free of surviving mutants.

The following survived mutants are intentionally permitted. Each is documented below with either a proof of behavioral equivalence or a rationale for why killing it would require disproportionate test complexity (e.g., reflection against private members) without practical benefit. No other Infrastructure mutant is permitted to survive outside this list; any new survivor must either be killed or added here with equivalent proof/rationale and reviewed.

### 1. Repository `CreateRangeAsync` Empty Check (`Count > 0` vs `Count >= 0`)

**Files:**
- [PatientPenaltyRepository.cs](../../ClinicFlow.Infrastructure/Persistence/Repositories/PatientPenaltyRepository.cs)
- [ScheduleRepository.cs](../../ClinicFlow.Infrastructure/Persistence/Repositories/ScheduleRepository.cs)

```csharp
public Task CreateRangeAsync(
    IEnumerable<PatientPenalty> penalties,
    CancellationToken cancellationToken = default
)
{
    var detachedPenalties = penalties
        .Where(p => dbContext.Entry(p).State is EntityState.Detached)
        .ToList();

    // Stryker mutates 'Count > 0' to 'Count >= 0'
    if (detachedPenalties.Count > 0)
        dbContext.PatientPenalties.AddRange(detachedPenalties);

    return Task.CompletedTask;
}
```

#### Mathematical and Operational Proof of Equivalence (Scoped to Real EF Core Behavior)
Stryker mutates the relational operator from `Count > 0` to `Count >= 0`.

1. **Behavior for `Count > 0` (1, 2, 3...):**
   Both `Count > 0` and `Count >= 0` evaluate to `true`. Both the original code and the mutant invoke `AddRange(detachedPenalties)` with the non-empty list. Their behavior is identical.

2. **Behavior for `Count == 0` (Empty List):**
   This is the only input where the two conditions differ:
   - **Original (`Count > 0`):** Evaluates to `false`. `AddRange(...)` is not invoked.
   - **Mutant (`Count >= 0`):** Evaluates to `true`. `AddRange([])` is invoked with an empty list.

3. **Indistinguishable Side Effects (Real EF Core Implementation Only):**
   In the real Entity Framework Core implementation, calling `AddRange` with an empty collection is an absolute no-op:
   - The `ChangeTracker` attaches zero entries.
   - No entity state changes occur.
   - No SQL is generated or queued.
   - No internal state is altered.

   Note: `DbSet<TEntity>.AddRange(IEnumerable<TEntity>)` is virtual in EF Core 10.0.11. This equivalence claim holds only for tests exercising the real EF Core implementation. A mock or derived `DbSet` could still observe the call itself (e.g. via `Verify(x => x.AddRange(...))`), even though no tracked or persisted state changes.

4. **Conclusion:**
   Restricted to tests that use the real EF Core implementation and assert only tracked or persisted state, `AddRange([])` and not calling `AddRange` produce the exact same observable outcome. Within that scope, the mutant is an **equivalent mutant** and cannot be killed by any assertion on tracked entities, persisted state, or generated SQL.
---

### 2. `ApplicationDbContext` Infrastructure Setup

**File:** [ApplicationDbContext.cs](../../ClinicFlow.Infrastructure/Persistence/ApplicationDbContext.cs)

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Stryker statement mutation: removes base.OnModelCreating(modelBuilder)
    base.OnModelCreating(modelBuilder);

    // Stryker string/statement mutation: mutates or removes extension registration
    modelBuilder.HasPostgresExtension("btree_gist");
    
    // Dynamic soft-delete filter expressions and sequence number conventions...
}
```

Mutants generated inside `OnModelCreating` survive due to the following characteristics:

1. **`base.OnModelCreating(modelBuilder)` Statement Removal:**
   In EF Core's base `DbContext` class, the virtual `OnModelCreating` method is an empty method (a complete no-op). When Stryker mutates this line by removing the invocation, calling an empty base method versus omitting the call produces zero side effects and leaves the model in an identical state. This makes the statement removal an **equivalent mutant** that cannot be distinguished by tests.

2. **Model Metadata and Engine Extensions:**
   Invocations such as `modelBuilder.HasPostgresExtension("btree_gist")` and dynamic query filter / row version metadata bindings target database engine extensions and ORM conventions. These configurations are exercised when applying database migrations against PostgreSQL, but have no branchable domain logic to verify in isolated unit or repository tests.

---

### 3. `UnitOfWork` Domain Events Filter (`Count > 0` vs `Count >= 0`)

**File:** [UnitOfWork.cs](../../ClinicFlow.Infrastructure/Persistence/UnitOfWork.cs)

```csharp
public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    var domainEntities = dbContext
        .ChangeTracker.Entries<BaseEntity>()
        .Where(x => x.Entity.DomainEvents.Count > 0) // Stryker mutates to 'Count >= 0'
        .ToList();

    var domainEvents = domainEntities.SelectMany(x => x.Entity.DomainEvents).ToList();

    foreach (var entity in domainEntities)
        entity.Entity.ClearDomainEvents();

    var result = await dbContext.SaveChangesAsync(cancellationToken);
    // Notification publishing follows...
}
```

#### Mathematical and Operational Proof of Equivalence
Stryker mutates `x.Entity.DomainEvents.Count > 0` to `x.Entity.DomainEvents.Count >= 0`.

1. **Entities with Events (`Count > 0`):**
   Both `Count > 0` and `Count >= 0` evaluate to `true`. Entities containing domain events are included in `domainEntities`, their events are collected into `domainEvents`, and their internal event collections are cleared.

2. **Entities without Events (`Count == 0`):**
   - **Original (`Count > 0`):** Evaluates to `false`. Entities with zero domain events are excluded from `domainEntities`.
   - **Mutant (`Count >= 0`):** Evaluates to `true`. Entities with an empty `DomainEvents` list are included in `domainEntities`.

3. **Indistinguishable Side Effects (Persistence & Notifications):**
   When an entity with 0 events is included:
   - `domainEntities.SelectMany(x => x.Entity.DomainEvents)` produces 0 items (no extra events added to `domainEvents`).
   - `entity.Entity.ClearDomainEvents()` on an already empty collection produces no observable difference in persisted data or published notifications. Note: internally,        `List<T>.Clear()` still increments the list's version counter even when empty, which could invalidate a concurrent enumerator over that same `DomainEvents` instance. No such concurrent enumeration occurs within the current `SaveChangesAsync` flow.
   - Dispatched notifications, database persistence, and entity states remain completely identical.

4. **Conclusion:**
   The output collection of events, the published MediatR notifications, and the database changes are identical. This is an **equivalent mutant** for the purposes of persisted state and published notifications, and cannot be distinguished by tests covering those concerns.

---

### 4. `UnitOfWork` Advisory Lock Key Conversion (`high ^ low` vs `~(high ^ low)`)

**File:** [UnitOfWork.cs](../../ClinicFlow.Infrastructure/Persistence/UnitOfWork.cs)

```csharp
private static long ToStableLong(Guid guid)
{
    Span<byte> bytes = stackalloc byte[16];
    guid.TryWriteBytes(bytes);
    var high = BitConverter.ToInt64(bytes[..8]);
    var low = BitConverter.ToInt64(bytes[8..]);
    return high ^ low; // Stryker mutates to '~(high ^ low)'
}
```

#### Rationale and Why This Mutant is Permitted to Survive
Stryker applies a bitwise mutation on `high ^ low`, converting it to `~(high ^ low)`.

1. **Behavioral Context:**
   `ToStableLong` is a `private static` helper function that compresses a 128-bit `Guid` into a 64-bit `long` to use as an integer key for PostgreSQL transaction-level advisory locks (`SELECT pg_advisory_xact_lock({key})`).

2. **Operational Effect on PostgreSQL:**
   Both `high ^ low` and `~(high ^ low)` produce a stable, deterministic 64-bit integer for any given `Guid`. PostgreSQL advisory locking behaves identically regardless of the specific integer value, provided it is stable per lock scope. Integration tests exercising `ExecuteWithLockAsync` verify transactional serialization and lock acquisition successfully under either computation.

3. **Testing Complexity and Trade-Off:**
   Because `ToStableLong` is private and static, killing this mutant would require either:
   - Invoking the private static method via **Reflection** in unit tests, which violates clean testing boundaries and adds fragile reflection plumbing.
   - Injecting complex EF Core command interceptors to inspect low-level raw SQL text and parameters sent to PostgreSQL.

4. **Conclusion:**
   Writing fragile reflection-based tests or complex database command interceptors to assert on an internal bitwise hashing formula adds significant maintenance complexity without improving business logic reliability. Therefore, this mutant is intentionally permitted to survive.

---

## Local Verification Protocol

Before submitting a Pull Request, contributors are expected to run Stryker locally on the affected test projects to ensure no regressions in test coverage.

### Running Stryker Locally

Navigate to the corresponding test project directory and run the Stryker tool:

```bash
# Domain Layer (fast, pure in-memory tests)
cd ClinicFlow.Domain.Tests
dotnet stryker

# Application Layer (fast, pure in-memory tests)
cd ClinicFlow.Application.Tests
dotnet stryker
```

#### Infrastructure Layer & Repository Testing

Infrastructure repository tests run against a real PostgreSQL container via **Testcontainers**. Running mutation testing across the entire Infrastructure project can take 10 to 20+ minutes on local machines, and multiple concurrent test runners can compete for connections and database resources within the container, potentially causing flaky test timeouts or false positives.

For this reason, **it is strongly recommended to target specific repository files with single concurrency (`--concurrency 1`)** when verifying infrastructure changes:

```bash
cd ClinicFlow.Infrastructure.Tests

# Target a specific repository with single concurrency
dotnet stryker -m "**/ScheduleRepository.cs" --concurrency 1

# Or target all repository implementations sequentially
dotnet stryker -m "**/Repositories/*.cs" --concurrency 1
```

### Reviewer and Contributor Checklist

When reviewing mutation test output:

1. **Verify Business Logic Mutants (Domain & Application):** Ensure all mutants in Domain entities, Domain services, and Application handlers/validators are killed (maintaining a 100% mutation score with zero surviving mutants).
2. **Inspect Survived Mutants (Infrastructure):** If any mutant survives in `ClinicFlow.Infrastructure`, verify whether it belongs strictly to the documented exceptions above (repository `CreateRangeAsync` boundary check, `ApplicationDbContext` model registration, `UnitOfWork` events filter, or `ToStableLong` bitwise helper). Outside of these documented exceptions, the Infrastructure layer must be free of surviving mutants.
3. **No Unjustified Mutants:** Any newly introduced survived mutant in business logic or repositories outside the documented exceptions must be addressed by adding corresponding test assertions before merging.
