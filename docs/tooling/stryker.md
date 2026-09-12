# Stryker Guidelines and Conventions

This document outlines the design decisions, conventions, and guidelines for using **Stryker.NET** (mutation testing) within the ClinicFlow project.

Configuration is managed through per-project `stryker-config.json` files:
- [Domain Tests Config](../../ClinicFlow.Domain.Tests/stryker-config.json)
- [Application Tests Config](../../ClinicFlow.Application.Tests/stryker-config.json)
- [Infrastructure Tests Config](../../ClinicFlow.Infrastructure.Tests/stryker-config.json)

For the CI workflow integration, see [.github/workflows/mutation-testing.yml](../../.github/workflows/mutation-testing.yml).

---

## Design Decisions & Philosophy

### 1. Domain-First Test Integrity Over Code Purity

Earlier revisions of this document enforced a strict "pure code" policy: no C# attributes (`[ExcludeFromCodeCoverage]`) and no Stryker suppression comments anywhere in Domain, Application, or Infrastructure. That policy has been **revised**, based on a recurring, well-understood pattern that the previous policy could not handle cleanly, and later extended to Infrastructure once the same reasoning proved to apply there too.

#### The problem the old policy created

ClinicFlow is a health system. Mutation testing is not a vanity metric here: it is one of the tools we rely on to catch real logical gaps before they become real bugs affecting patient data. A mutation report is only useful if a survived mutant reliably means "there might be a real gap here." Under the old policy, that signal was drowned out by a specific, repeating, structurally unavoidable false positive in Domain:

```csharp
public string TemplateCode { get; private set; } = string.Empty;
```

Properties with a private setter, populated exclusively through a validated factory method or constructor, still carry a default value assigned inline. Stryker has no way to know that the default is unreachable in practice: it mutates the literal (`string.Empty` → `"Stryker was here!"`) and generates a mutant that **cannot be killed by any reasonable test**, because reaching it would require asserting on a transient default value that the factory always overwrites before the object is ever usable.

With every regeneration of the report, these false positives reappeared indistinguishably alongside genuine survivors. Verifying each one ("is this the known false positive, or a real gap this time?") for every survived mutant, every run, is exactly the kind of manual, error-prone triage that erodes trust in the tool and, eventually, gets skipped under deadline pressure. That is the real risk: not that the score looks worse than it is, but that a genuine gap gets waved through because it looked like the familiar noise.

#### The revised policy

`// Stryker disable once <mutator>` comments are now permitted, under a single condition: the mutant must be genuinely unreachable by design, or proven behaviorally equivalent, or a documented, justified cost/benefit trade-off, never suppressed merely because writing the test is inconvenient. The comment is a deliberate, per-line, per-mutator decision, never a blanket suppression, and it exists to keep the report's signal clean, not to inflate the score.

This is a conscious trade-off. We are accepting a small amount of tooling annotation inside source files in exchange for a report where a survived mutant reliably means "look here." We are explicitly **not** doing this to chase a 100% score: a suppressed mutant does not count toward the score at all (see [Effect on Mutation Score](#effect-on-mutation-score)); it is removed from consideration entirely, same as an `Ignored` mutant from the `mutate` config exclusions.

Every suppression must be documented at the point of use with a reason, and, for anything beyond the trivial Domain pattern below, cross-referenced in [Documented Survived & Equivalent Mutants](#documented-survived--equivalent-mutants), so the justification lives in git history and PR review, not just in a one-line comment.

#### Why per-line `disable once`, not `disable all` / `restore all`

We initially considered using ranged suppression to cover multiple adjacent properties with a single pair of comments:

```csharp
// Stryker disable all: props overwritten by factory method
public string Name { get; private set; } = string.Empty;
public string Description { get; private set; } = string.Empty;
// Stryker restore all
```

**This does not reliably work in Stryker.NET.** We verified directly against our own codebase that a mutant between a `disable all`/`restore all` pair can still survive uninhibited, confirmed against known upstream issues in the Stryker.NET repository describing the same failure (ranged `disable`/`restore all` not consistently respected, particularly across separate member declarations). Do not use `disable all` / `restore all` for this purpose. It is not a matter of syntax mistakes; the feature has known reliability gaps in this scenario.

The reliable pattern is `disable once`, scoped to the specific mutator, applied individually above each affected declaration:

```csharp
// Stryker disable once String
public string Name { get; private set; } = string.Empty;

// Stryker disable once String
public string Description { get; private set; } = string.Empty;
```

Yes, this means one comment per property, with no shortcut for grouping several at once. That repetition is the accepted cost of a suppression mechanism we can actually trust, confirmed working, run after run, over one that silently fails a fraction of the time.

#### Domain: when this applies (and when it does not)

Two recurring, pre-approved cases in Domain:

1. **Properties with a private setter and a literal default, populated only through a factory method or constructor that fully replaces the default before the entity is usable.**
   ```csharp
   // Stryker disable once String
   public string Code { get; private set; } = string.Empty;
   ```
   Mutator: `String`. Reason: the mutated default is never observable outside object construction.

2. **Private, parameterless constructors that exist solely for EF Core materialization and are never invoked by application code.**
   ```csharp
   // Stryker disable once all
   // EF Core constructor
   private AppointmentTypeDefinition()
   {
       // ...
   }
   ```
   Mutator: `all` (there is no specific mutator to isolate; the whole construct is unreachable by design). Reason: EF Core invokes this exclusively via reflection during materialization; no application-level test can reach it without instantiating a real `DbContext` and asserting on ORM internals, which would test EF Core's behavior, not ours.

This exception does **not** cover:
- Any property or method with real branching, validation, or domain logic.
- A survived mutant simply because writing the test is inconvenient or time-consuming.
- Anything where killing the mutant would actually close a real gap: those get a test, not a comment.

Before adding `// Stryker disable once` to a new case, confirm it is genuinely unreachable by the same reasoning as the two cases above. If in doubt, treat it as a real gap and write the test.

#### Infrastructure: same mechanism, individually justified cases

The four previously-documented Infrastructure survivors ([CreateRangeAsync empty check](#1-repository-createrangeasync-empty-check-count--0-vs-count--0), [ApplicationDbContext setup](#2-applicationdbcontext-infrastructure-setup), [UnitOfWork events filter](#3-unitofwork-domain-events-filter-count--0-vs-count--0), and [ToStableLong bitwise conversion](#4-unitofwork-advisory-lock-key-conversion-high--low-vs-high--low)) now also carry `// Stryker disable once` comments at their exact location, pointing back to their full proof or rationale below. Unlike the Domain pattern, these are **not** a repeating structural case: each is a unique, individually reviewed piece of logic, and each one's comment exists only because the accompanying proof or rationale in this document already justified it in full. The comment is a pointer to that justification, not a replacement for it. Any new Infrastructure survivor must go through the same documentation process (a full proof of equivalence or an explicit cost/benefit rationale) before it is suppressed; it is never suppressed on the strength of the comment alone.

#### Application: no suppressions needed

Unlike Domain and Infrastructure, the Application layer currently has no documented suppressions and needs none. Its 100% score is achieved purely through test coverage, not through any `// Stryker disable` comment. This isn't a gap in the exception process, it simply reflects that Application code (CQRS handlers, validators, mapping logic) doesn't exhibit the structural patterns that make Domain and Infrastructure need suppression: no factory-populated default properties, no EF Core materialization constructors, no equivalent-mutant arithmetic. If a genuinely unreachable or equivalent mutant ever does surface in Application, it follows the same documentation process as Infrastructure: a full proof or rationale added to this document before any suppression comment is added to the code.

#### Effect on mutation score

A mutant marked `Ignored` via `// Stryker disable` is **removed from the score calculation entirely**, same treatment as mutants excluded via the `mutate` glob patterns. It is not counted as killed, and it is not counted as survived; it simply does not enter the denominator. This is different from `NoCoverage`, which **does** count against the score (a `NoCoverage` mutant is treated as equivalent to a survivor, since no test exercises that line at all). Suppressing a genuinely unreachable EF Core constructor moves it from `NoCoverage` (penalizing the score) to `Ignored` (neutral): a legitimate correction, not score inflation, because the code is not testable by design in the first place.

### 2. Non-Blocking CI Status & Baseline Drift
Stryker mutation testing runs in CI via GitHub Actions, but **it is intentionally configured as a non-blocking check for Pull Requests**.

This decision was made due to recurring tooling limitations in incremental baseline mode:

**Baseline Reset on Accumulated Edits:**
Stryker's `--with-baseline` mode operates by only evaluating mutants within changed code and comparing against a base commit. When a file accumulates modifications across multiple commits or refactorings, Stryker resets the baseline status for all mutants in the modified blocks. Consequently, previously killed or suppressed mutants can lose their baseline status and resurface as survived mutants in CI reports without any real regression in business logic.

Due to this baseline inconsistency, Stryker is not a required gating check in PRs. Instead, developers verify mutation results locally before pushing. Local runs execute a full (non-baseline) analysis, so new mutants are not automatically isolated from pre-existing ones. Developers should review any survived mutants and confirm whether they require a new documented suppression (see above) or represent a real gap in business logic before merging.

### 3. Mutation Score Thresholds & Current Status
Each project defines three mutation score thresholds in its `stryker-config.json`:

| Project | High (Target) | Low (Warning) | Break (Failure) | Current Status |
|---|---|---|---|---|
| **Domain** | ≥ 95% | 90% | < 85% | **100% (0 surviving mutants)** |
| **Application** | ≥ 95% | 90% | < 80% | **100% (0 surviving mutants)** |
| **Infrastructure** | ≥ 95% | 90% | < 85% | **100% (0 surviving mutants)** |

**Domain**, **Application**, and **Infrastructure** all achieve and maintain a **100% mutation score with zero surviving mutants**. Every mutation generated across domain entities, value objects, domain services, CQRS command/query handlers, validators, repository implementations, persistence logic, and policies is either killed by the corresponding test suite or deliberately suppressed via a documented `// Stryker disable once` comment, per the policy above.

Suppressed mutants are not hidden from scrutiny: every suppression in Infrastructure is backed by a full proof of equivalence or an explicit rationale in [Documented Survived & Equivalent Mutants](#documented-survived--equivalent-mutants), and any newly introduced suppression must be added there and reviewed before merging.

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
> **Zero Survived Mutants Across All Layers:**
> `ClinicFlow.Domain`, `ClinicFlow.Application`, and `ClinicFlow.Infrastructure` all have **zero surviving mutants** (100% mutation score). Mutants that would otherwise survive are either killed by tests or intentionally suppressed via `// Stryker disable once` comments, each backed by the proof or rationale below.

The following mutants are intentionally suppressed via code comment. Each is documented below with either a proof of behavioral equivalence or a rationale for why killing it would require disproportionate test complexity (e.g., reflection against private members) without practical benefit. No new suppression is permitted outside this process; any newly introduced survivor must either be killed, or added here with equivalent proof/rationale, reviewed, and only then suppressed in code.

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

    // Stryker disable once Equality: see docs/tooling/stryker.md, section "1. Repository CreateRangeAsync Empty Check"
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
   Restricted to tests that use the real EF Core implementation and assert only tracked or persisted state, `AddRange([])` and not calling `AddRange` produce the exact same observable outcome. Within that scope, the mutant is an **equivalent mutant** and cannot be killed by any assertion on tracked entities, persisted state, or generated SQL. Suppressed via `// Stryker disable once Equality`.

---

### 2. `ApplicationDbContext` Infrastructure Setup

**File:** [ApplicationDbContext.cs](../../ClinicFlow.Infrastructure/Persistence/ApplicationDbContext.cs)

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Stryker disable once all: see docs/tooling/stryker.md, section "2. ApplicationDbContext Infrastructure Setup"
    base.OnModelCreating(modelBuilder);

    // Stryker disable once String: see docs/tooling/stryker.md, section "2. ApplicationDbContext Infrastructure Setup"
    modelBuilder.HasPostgresExtension("btree_gist");

    // Dynamic soft-delete filter expressions and sequence number conventions...
}
```

Mutants generated inside `OnModelCreating` are suppressed due to the following characteristics:

1. **`base.OnModelCreating(modelBuilder)` Statement Removal:**
   In EF Core's base `DbContext` class, the virtual `OnModelCreating` method is an empty method (a complete no-op). When Stryker mutates this line by removing the invocation, calling an empty base method versus omitting the call produces zero side effects and leaves the model in an identical state. This makes the statement removal an **equivalent mutant** that cannot be distinguished by tests.

2. **Model Metadata and Engine Extensions:**
   Invocations such as `modelBuilder.HasPostgresExtension("btree_gist")` and dynamic query filter / row version metadata bindings target database engine extensions and ORM conventions. These configurations are exercised when applying database migrations against PostgreSQL, but have no branchable domain logic to verify in isolated unit or repository tests.

This exception is strictly limited to the two lines above. It does **not** extend to the `foreach` loops configuring `SequenceNumber`/`Version` (`BaseEntity`) or the dynamically-built soft-delete query filter (`SoftDeletableEntity`): both are exercised by `ApplicationDbContextModelTests` and `ApplicationDbContextIntegrationTests` respectively, and any survived mutants there represent real gaps and must not be suppressed.

---

### 3. `UnitOfWork` Domain Events Filter (`Count > 0` vs `Count >= 0`)

**File:** [UnitOfWork.cs](../../ClinicFlow.Infrastructure/Persistence/UnitOfWork.cs)

```csharp
public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
   // Stryker disable once Equality: see docs/tooling/stryker.md, section "3. UnitOfWork Domain Events Filter"
    var domainEntities = dbContext
        .ChangeTracker.Entries<BaseEntity>()
        .Where(x => x.Entity.DomainEvents.Count > 0)
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
   - `entity.Entity.ClearDomainEvents()` on an already empty collection produces no observable difference in persisted data or published notifications. Note: internally, `List<T>.Clear()` still increments the list's version counter even when empty, which could invalidate a concurrent enumerator over that same `DomainEvents` instance. No such concurrent enumeration occurs within the current `SaveChangesAsync` flow.
   - Dispatched notifications, database persistence, and entity states remain completely identical.

4. **Conclusion:**
   The output collection of events, the published MediatR notifications, and the database changes are identical. This is an **equivalent mutant** for the purposes of persisted state and published notifications, and cannot be distinguished by tests covering those concerns. Suppressed via `// Stryker disable once Equality`.

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
    // Stryker disable once Bitwise: see docs/tooling/stryker.md, section "4. UnitOfWork Advisory Lock Key Conversion"
    return high ^ low;
}
```

#### Rationale and Why This Mutant is Suppressed
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
   Writing fragile reflection-based tests or complex database command interceptors to assert on an internal bitwise hashing formula adds significant maintenance complexity without improving business logic reliability. Therefore, this mutant is intentionally suppressed via `// Stryker disable once Bitwise`.

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

1. **Verify Business Logic Mutants:** Ensure all mutants across Domain, Application, and Infrastructure are killed or suppressed, maintaining a 100% mutation score with zero surviving mutants.
2. **Inspect Any `// Stryker disable` Comment:** Confirm it falls into one of the pre-approved Domain patterns (factory-populated property default, EF Core materialization constructor) or references an entry in [Documented Survived & Equivalent Mutants](#documented-survived--equivalent-mutants). A suppression comment with no corresponding documentation entry (for anything beyond the trivial Domain pattern) should be rejected in review.
3. **No Unjustified Suppressions:** Any newly introduced suppression outside the pre-approved Domain patterns must come with a full proof of equivalence or an explicit cost/benefit rationale added to this document before merging. If in doubt, write the test instead of suppressing the mutant.