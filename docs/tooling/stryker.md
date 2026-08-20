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
   Lines in EF Core configuration such as `modelBuilder.HasPostgresExtension("btree_gist")` or metadata loops configure database extensions and entity conventions. Mutants generated on string literals or method calls in these configuration sections have no observable branching logic to test via unit assertions, yet Stryker reports them as survived mutants (0% score).

2. **Baseline Reset on Accumulated Edits:**
   Stryker's `--with-baseline` mode operates by only evaluating mutants within changed code and comparing against a base commit. When a file accumulates modifications across multiple commits or refactorings, Stryker resets the baseline status for all mutants in the modified blocks. Consequently, previously killed or benign configuration mutants can lose their baseline status and resurface as survived mutants in CI reports without any real regression in business logic.

Due to these false positives and baseline inconsistencies, Stryker is not a required gating check in PRs. Instead, developers verify mutation results locally before pushing, confirming that no new unkilled mutants are introduced into actual business logic.

### 3. Mutation Score Thresholds
Each project defines three mutation score thresholds in its `stryker-config.json`:

| Project | High (Target) | Low (Warning) | Break (Failure) |
|---|---|---|---|
| **Domain** | ≥ 95% | 90% | < 85% |
| **Application** | ≥ 95% | 90% | < 80% |
| **Infrastructure** | ≥ 95% | 90% | < 85% |

---

## Configured Exclusions

Certain file patterns and namespaces are globally excluded from mutation analysis in each `stryker-config.json` because they represent pure data contracts, DI registrations, or boilerplate infrastructure without testable branching logic.

### Domain Layer (`ClinicFlow.Domain.Tests/stryker-config.json`)
- **`**/Enums/**`:** Enum declarations.
- **`**/Interfaces/**`:** Interface definitions.
- **`**/Events/**`:** Simple data-carrying domain event records.
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

The following survived mutants are intentionally permitted because they represent **equivalent mutants** (mutants whose behavior is mathematically and operationally indistinguishable from the original code) or pure configuration lines.

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

#### Mathematical and Operational Proof of Equivalence

Stryker mutates the relational operator from `Count > 0` to `Count >= 0`.

1. **Behavior for `Count > 0` (1, 2, 3...):**
   Both `Count > 0` and `Count >= 0` evaluate to `true`. Both the original code and the mutant invoke `AddRange(detachedPenalties)` with the non-empty list. Their behavior is identical.

2. **Behavior for `Count == 0` (Empty List):**
   This is the only input where the two conditions differ:
   - **Original (`Count > 0`):** Evaluates to `false`. `AddRange(...)` is not invoked.
   - **Mutant (`Count >= 0`):** Evaluates to `true`. `AddRange([])` is invoked with an empty list.

3. **Indistinguishable Side Effects:**
   In Entity Framework Core, calling `AddRange` with an empty collection is an absolute no-op:
   - The `ChangeTracker` attaches zero entries.
   - No entity state changes occur.
   - No SQL is generated or queued.
   - No internal state is altered.

4. **Conclusion:**
   Because calling `AddRange([])` and not calling `AddRange` produce the exact same final state, there is no observable outcome or assertion in any unit or integration test that can differentiate the original code from the mutant. The mutant is an **equivalent mutant** and cannot be killed.

---

### 2. `ApplicationDbContext` Infrastructure Setup

**File:** [ApplicationDbContext.cs](../../ClinicFlow.Infrastructure/Persistence/ApplicationDbContext.cs)

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Stryker mutates the extension name or removes the invocation
    modelBuilder.HasPostgresExtension("btree_gist");
    
    // Dynamic soft-delete filter expressions and sequence number conventions...
}
```

Mutants generated inside `OnModelCreating` target database engine extensions and model metadata bindings. These configurations are exercised during database schema creation and integration migrations, but have no branchable business logic to verify in isolated unit tests.

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

1. **Verify Business Logic Mutants:** Ensure all mutants in Domain entities, Domain services, and Application handlers/validators are killed (100% mutation score on business logic).
2. **Inspect Survived Mutants:** If any mutant survives, verify whether it belongs to the documented equivalent mutants (e.g. `CreateRangeAsync` boundary check) or pure configuration lines.
3. **No Unjustified Mutants:** Any newly introduced survived mutant in business logic must be addressed by adding corresponding test assertions before merging.
