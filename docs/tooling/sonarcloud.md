# SonarCloud Guidelines and Conventions

This document outlines the design decisions, conventions, and guidelines for using **SonarCloud** within the ClinicFlow project.

Configuration is managed entirely through the SonarCloud web UI and CLI parameters in the CI pipeline. There is no local configuration file. See the [ci.yml](../../.github/workflows/ci.yml) file for the pipeline integration.

---

## Design Decisions & Philosophy

### 1. No `sonar-project.properties` File

The .NET scanner (`dotnet-sonarscanner`) is incompatible with `sonar-project.properties`. The scanner generates its own properties file internally from the `.sln` and `.csproj` files, and explicitly fails if it finds a manual `sonar-project.properties` in the repository:

```
sonar-project.properties files are not understood by the SonarScanner for .NET.
Remove those files from the following folders: /home/runner/work/ClinicFlow/ClinicFlow
Post-processing failed. Exit code: 1
```

For this reason, all SonarCloud configuration is managed through:

- **CLI parameters** in the `dotnet-sonarscanner begin` step (project key, organization, coverage report paths).
- **SonarCloud web UI** settings (exclusion patterns, rule customizations, Quality Gate).

### 2. Pure Domain and Application Code

Consistent with the [Codecov philosophy](./codecov.md), **we do not use `// NOSONAR` comments or `[SuppressMessage]` attributes** anywhere in the Domain or Application layers. The codebase must remain free from tooling-specific annotations. When SonarCloud reports a false positive on clean domain code, we resolve it through the SonarCloud web UI rather than polluting the source.

### 3. Coverage Source

SonarCloud consumes the same OpenCover XML report (`coverage.opencover.xml`) generated during the test step in CI. Coverage is not collected separately. Both Codecov and SonarCloud share the same test execution and report artifacts.

### 4. No Shared Abstractions in the Application Layer

Command and query handlers, and their validators, never inherit from a common base class or delegate to a shared service purely to eliminate structural repetition. Each handler is an independent, self-contained unit that fetches its own dependencies and orchestrates its own business rules — even when that means repeating a similar-looking sequence of lookups, validations, or persistence calls across handlers.

This is a conscious tradeoff based on past experience: shared handler abstractions (base classes, generic lookup services) led to code that was harder to read, harder to change safely, and coupled use cases that were conceptually unrelated. A change to one handler's shared base could silently ripple into handlers that had nothing to do with it. Duplication that is a **natural consequence of consistent handler design** is accepted; duplication is only actually addressed when it represents genuine copy-pasted business logic, not structural similarity between independent use cases.

This philosophy directly informs the [Duplication Exclusions](#duplication-exclusions-sonarcpdexclusions) below.

---

## Quality Gate

The project uses the **Sonar way** default Quality Gate, which enforces the following thresholds on new code:

| Metric | Threshold |
|---|---|
| Coverage on New Code | ≥ 80% |
| Duplicated Lines on New Code | ≤ 3% |
| New Bugs Rating | ≤ A |
| New Vulnerabilities Rating | ≤ A |
| New Security Hotspots Rating | ≤ A |
| New Code Smells Rating | ≤ A |

The Quality Gate is configured as a **required check** for Pull Requests. Failing it blocks the merge.

---

## Configured Exclusions

### Coverage Exclusions (`sonar.coverage.exclusions`)

These patterns are excluded from coverage analysis because they do not contain testable business logic:

- **Domain Events (`**/Events/**/*.cs`):** Simple data-carrying records with no business logic, verified indirectly through the entities that raise them.
- **`ApplicationDbContextFactory.cs`:** Design-time factory used exclusively for EF Core CLI migrations.
- **`ApplicationDbContext.cs`:** DbContext configuration with no testable business logic.
- **EF Core Configurations (`**/Configurations/**`):** Fluent API entity configurations.
- **Dependency Injection ([DependencyInjection.cs](../../ClinicFlow.Application/DependencyInjection.cs) / [DependencyInjection.cs](../../ClinicFlow.Infrastructure/DependencyInjection.cs)):** Service registration files for DI container (pure registration boilerplate, no testable business logic).

### Duplication Exclusions (`sonar.cpd.exclusions`)

These patterns are excluded from Copy-Paste Detection because their structural similarity is intentional by design:

- **CQRS Command Handlers (`**/Commands/**/*CommandHandler.cs`) and Query Handlers (`**/Queries/**/*QueryHandler.cs`):** Consistent with the project's philosophy of [no shared abstractions in the Application layer](#4-no-shared-abstractions-in-the-application-layer), command and query handlers are never refactored into a shared base class or lookup service to satisfy the duplication detector. Any structural similarity flagged between handlers is expected and accepted, not a defect. This blanket exclusion supersedes the narrower, handler-by-handler exclusions previously maintained here — new handlers are covered automatically and do not require individual exclusion requests.

  Representative examples of this pattern in practice:
  - **Reschedule Command Handlers** (`RescheduleByDoctorCommandHandler`, `RescheduleByStaffCommandHandler`, `RescheduleByPatientCommandHandler`) orchestrate the same rescheduling workflow but each delegates to a distinct domain service method enforcing different authorization rules depending on who initiates the reschedule.
  - **AppointmentType Query Handlers** (`GetAllActiveAppointmentTypesQueryHandler`, `GetAppointmentTypeByIdQueryHandler`, `GetAppointmentTypesByCategoryQueryHandler`, `GetEligibleAppointmentTypesQueryHandler`) follow the same fetch → project → return orchestration, a natural consequence of consistent handler design rather than copy-paste.
  - **Family Member Command Handlers** (`AddFamilyMemberCommandHandler`, `AddCompleteFamilyMemberCommandHandler`) share nearly identical concurrency locking and pre-validation checks before diverging in what they persist.
  - **Medical Encounter Command Handlers** (`StartMedicalEncounterCommandHandler`, `CompleteMedicalEncounterCommandHandler`) share the same doctor/appointment/appointmentType lookup sequence before diverging into starting vs. completing the encounter.

- **Command Validators (`**/Commands/**/*Validator.cs`):** Validators were originally built on shared base classes and interfaces (e.g., `RegisterUserCommandValidatorBase<T>`, `CancelCommandValidatorBase<T>`). These abstractions were removed in favor of standalone validators to eliminate unnecessary coupling and forced property contracts. The resulting validators share structural patterns inherent to the FluentValidation API, which triggers the duplication detector despite each validator being independently authored.
- **`PatientPenalty.cs`:** Contains intentionally duplicated factory methods (`CreateAutomaticBlock` / `CreateManualBlock`) that preserve explicit domain intent despite identical implementations.

- **EF Core Configurations (`**/Configurations/**`):** Fluent API configurations with repetitive structural patterns.
- **`ApplicationDbContextFactory.cs` / `ApplicationDbContext.cs`:** Infrastructure boilerplate.
- **Seeding (`**/Seeding/**`):** Data seeding code with repetitive builder patterns.

### File Exclusions (`sonar.exclusions`)
These patterns are completely excluded from analysis: no issues, no coverage, no duplication check:

- **Migrations (`**/Migrations/**`):** Entity Framework Core migration history. Auto-generated code with no testable business logic, triggers false-positive-prone rules like `CA1861` (prefer static readonly array) that are inherent to how the EF Core scaffolder writes `Up()`/`Down()` methods.

---

## Resolved False Positives

The following SonarCloud issues have been marked as **False Positive** in the web UI with documented justifications:

### `CA1859`. Use concrete type for improved performance

**File:** `AppointmentGenerator.cs` (Infrastructure/Persistence/Seeding)

```
Change type of field '_patientUsersById' from 'IReadOnlyDictionary' to 'Dictionary' for improved performance
```

**Resolution:** `IReadOnlyDictionary` is intentional. It enforces immutability at compile time. The marginal performance gain from using a concrete type does not justify losing the compile-time safety guarantee.

### `S2068`. Hard-coded credentials

**File:** `ApplicationDbContextFactory.cs` (Infrastructure/Persistence)

```
"password" detected here, make sure this is not a hard-coded credential.
```

**Resolution:** This is a design-time factory for EF Core CLI migrations only. The connection string is intentional for local development. This class is not used in production and will be removed once the API layer provides its own DI configuration.

### `S107`. Too many constructor parameters

**Files:** `CancelAppointmentByPatientCommandHandler.cs`, `RescheduleByPatientCommandHandler.cs`, `ScheduleByPatientCommandHandler.cs` (Application)

```
Constructor has 8 or 9 parameters, which is greater than the 7 authorized.
```

**Resolution:** Intentional by design. These are CQRS orchestration handlers where each of the dependencies serves a distinct, non-mergeable responsibility. Refactoring into facade or aggregate services would obscure dependencies without reducing actual complexity.

**File:** `AppointmentTypeDefinition.cs` (Domain/Entities)

```
Constructor has 8 parameters, which is greater than the 7 authorized.
```

**Resolution:** Intentional by design. The private constructor captures the complete initial state of this core entity, where each parameter maps to a distinct, non-mergeable invariant. Wrapping them in a DTO or parameter object would only camouflage the count without reducing actual complexity, while obscuring the factory intent. Same reasoning as the Application handler `S107` cases above.

### `S4144`. Identical method implementations

**File:** `PatientPenalty.cs` (Domain/Entities)

```
Update this method so that its implementation is not identical to 'CreateAutomaticBlock'.
```

**Resolution:** `CreateManualBlock` intentionally duplicates `CreateAutomaticBlock` to preserve explicit domain intent. Each factory method represents a distinct business concept. Merging them would sacrifice domain clarity for mechanical deduplication.

### `S1186`. Empty method
**File:** `20260706041849_MoveDynamicClinicalDetailToEntities.cs` (Infrastructure/Persistence/Migrations)
**PR:** [#320](https://github.com/0Crazy-0/ClinicFlow/pull/320)

**Resolution:** The `Down()` method generated by EF Core migrations is intentionally left empty when a migration is not designed to be reversible. A comment documenting this (`EF Core migration Down method, not reversible by design`) satisfies the intent of the rule without requiring a `NotSupportedException` or artificial implementation.

> **Note:** This was originally marked as a false positive before the entire `Migrations` directory was excluded from analysis via `sonar.exclusions` (Source File Exclusions). It is documented here as the individual case that first surfaced the pattern. The directory-wide exclusion now makes migration files invisible to the analyzer entirely, superseding the narrower `S1186`-specific and coverage/duplication-specific exclusions, along with any other rule that could otherwise trigger on generated migration code.

---

## Resolution Protocol for Failing Quality Gate

The SonarCloud Quality Gate is a **required check**. It cannot be overridden or bypassed. If it fails, the Pull Request cannot be merged. Period.

Resolution is handled through the SonarCloud web UI by inspecting the specific issues reported on the Pull Request.

### Genuine Issues. Fix in the PR Branch

If the reported issue is legitimate, it must be corrected with a commit on the same Pull Request branch:

- **Missing coverage:** Add unit tests that cover the new or modified business logic.
- **Code duplication:** Refactor the duplicated code to eliminate the structural repetition.
- **Bugs, vulnerabilities, security hotspots, or code smells:** Fix the issue in the source code.

### False Positives. Mark in SonarCloud UI

If the reported issue is a false positive, the resolution is managed entirely through the SonarCloud web UI, never by modifying source code to appease the tool.

**Who can mark false positives:** Only the project administrator. Contributors cannot mark issues as false positives directly.

**Contributor workflow:**

- Identify the issue in the SonarCloud PR report.
- Leave a comment on the PR with a justification explaining why the issue is a false positive.
- Request the project administrator to review and, if the justification is reasonable, mark it as a false positive in the SonarCloud UI with the provided rationale as a comment.

**Administrator workflow:**

- Review the contributor's justification.
- If reasonable, navigate to the issue in the SonarCloud web UI.
- Mark the issue as **False Positive**.
- Add a comment on the issue documenting the rationale.

### Requesting Exclusion Additions

In some cases, a Quality Gate failure is caused by files or directories that should have been excluded from analysis from the start (e.g., new EF Core configuration classes, auto-generated code, or infrastructure boilerplate with no testable logic). This applies to both **coverage exclusions** and **duplication exclusions**.

When this is the case:

- The contributor must identify the file or directory pattern that should be excluded.
- Request the project administrator to add it to the corresponding exclusion list in the SonarCloud web UI.
- Once added, re-run the analysis to verify the Quality Gate passes.

### Permissible False-Positive Scenarios

The same false-positive scenarios documented for [Codecov](./codecov.md#permissible-false-positive-scenarios) apply to SonarCloud's coverage gate (EF Core parameterless constructors, Application DTOs).

Additionally, the following SonarCloud-specific scenario may cause legitimate Quality Gate failures:

#### Duplication on New CQRS Handlers
New command and query handlers are covered automatically by the blanket `**/Commands/**/*CommandHandler.cs` and `**/Queries/**/*QueryHandler.cs` exclusions (see [Duplication Exclusions](#duplication-exclusions-sonarcpdexclusions)) and do not require individual exclusion requests. If a new handler triggers duplication against non-handler code (unlikely, but possible), evaluate it on its own merits rather than assuming the blanket exclusion applies.

---

## Rejected Changes & Closed PRs

### Adding `sonar-project.properties` (PR [#242](https://github.com/0Crazy-0/ClinicFlow/pull/242))
**Issue:** An attempt to add a local `sonar-project.properties` file. As detailed in [No sonar-project.properties File](#1-no-sonar-projectproperties-file), this file is incompatible with the .NET scanner and causes CI to fail immediately.
**Resolution:** PR closed. No exclusion or workaround applies. The file must not exist in the repository.

### Parameter Renaming in Interface Implementations (Rule S927) (PR [#191](https://github.com/0Crazy-0/ClinicFlow/pull/191))
**Issue:** Renaming parameters in implemented methods (e.g., `cancellationToken` → `ct` in MediatR handlers) violates rule S927, which requires implementation parameter names to match the interface declaration.
**Resolution:** PR closed. Cosmetic renames that violate S927 are not accepted, and the rule is not suppressed for stylistic changes.
