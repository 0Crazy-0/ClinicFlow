# SonarCloud Guidelines and Conventions

This document outlines the design decisions, conventions, and guidelines for using **SonarCloud** within the ClinicFlow project.

Configuration is managed entirely through the SonarCloud web UI and CLI parameters in the CI pipeline — there is no local configuration file. See the [ci.yml](../../.github/workflows/ci.yml) file for the pipeline integration.

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

SonarCloud consumes the same OpenCover XML report (`coverage.opencover.xml`) generated during the test step in CI. Coverage is not collected separately — both Codecov and SonarCloud share the same test execution and report artifacts.

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

The Quality Gate is configured as a **required check** for Pull Requests — failing it blocks the merge.

---

## Configured Exclusions

### Coverage Exclusions (`sonar.coverage.exclusions`)

These patterns are excluded from coverage analysis because they do not contain testable business logic:

- **Domain Events (`**/Events/**/*.cs`):** Simple data-carrying records with no business logic, verified indirectly through the entities that raise them.
- **Migrations (`**/Migrations/**`):** Entity Framework Core migration history.
- **`ApplicationDbContextFactory.cs`:** Design-time factory used exclusively for EF Core CLI migrations.
- **`ApplicationDbContext.cs`:** DbContext configuration with no testable business logic.
- **EF Core Configurations (`**/Configurations/**`):** Fluent API entity configurations.

### Duplication Exclusions (`sonar.cpd.exclusions`)

These patterns are excluded from Copy-Paste Detection because their structural similarity is intentional by design:

- **Reschedule Command Handlers:** `RescheduleByDoctorCommandHandler.cs`, `RescheduleByStaffCommandHandler.cs` — These handlers orchestrate the same rescheduling workflow but intentionally remain separate. Each delegates to a distinct domain service method that enforces different authorization and business rules depending on who initiates the reschedule (doctor vs. staff). A generic handler was explicitly rejected to keep each handler readable and independently evolvable as requirements grow.
- **AppointmentType Query Handlers:** `GetAllActiveAppointmentTypesQueryHandler.cs`, `GetAppointmentTypeByIdQueryHandler.cs`, `GetAppointmentTypesByCategoryQueryHandler.cs`, `GetEligibleAppointmentTypesQueryHandler.cs` — These handlers follow the same CQRS orchestration pattern (fetch → project → return). Their structural similarity is a natural consequence of consistent handler design, not duplication.
- **Command Validators (`**/Commands/**/*Validator.cs`):** Validators were originally built on shared base classes and interfaces (e.g., `RegisterUserCommandValidatorBase<T>`, `CancelCommandValidatorBase<T>`). These abstractions were removed in favor of standalone validators to eliminate unnecessary coupling and forced property contracts. The resulting validators share structural patterns inherent to the FluentValidation API, which triggers the duplication detector despite each validator being independently authored.
- **`PatientPenalty.cs`:** Contains intentionally duplicated factory methods (`CreateAutomaticBlock` / `CreateManualBlock`) that preserve explicit domain intent despite identical implementations.
- **Migrations (`**/Migrations/**`):** Auto-generated migration code.
- **EF Core Configurations (`**/Configurations/**`):** Fluent API configurations with repetitive structural patterns.
- **`ApplicationDbContextFactory.cs` / `ApplicationDbContext.cs`:** Infrastructure boilerplate.
- **Seeding (`**/Seeding/**`):** Data seeding code with repetitive builder patterns.

---

## Resolved False Positives

The following SonarCloud issues have been marked as **False Positive** in the web UI with documented justifications:

### `CA1859` — Use concrete type for improved performance

**File:** `AppointmentGenerator.cs` (Infrastructure/Persistence/Seeding)

```
Change type of field '_patientUsersById' from 'IReadOnlyDictionary' to 'Dictionary' for improved performance
```

**Resolution:** `IReadOnlyDictionary` is intentional — it enforces immutability at compile time. The marginal performance gain from using a concrete type does not justify losing the compile-time safety guarantee.

### `S2068` — Hard-coded credentials

**File:** `ApplicationDbContextFactory.cs` (Infrastructure/Persistence)

```
"password" detected here, make sure this is not a hard-coded credential.
```

**Resolution:** This is a design-time factory for EF Core CLI migrations only. The connection string is intentional for local development. This class is not used in production and will be removed once the API layer provides its own DI configuration.

### `S107` — Too many constructor parameters

**Files:** `RescheduleByPatientCommandHandler.cs`, `ScheduleByPatientCommandHandler.cs` (Application)

```
Constructor has 9 parameters, which is greater than the 7 authorized.
```

**Resolution:** Intentional by design. These are CQRS orchestration handlers where each of the 9 dependencies serves a distinct, non-mergeable responsibility. Refactoring into facade or aggregate services would obscure dependencies without reducing actual complexity.

### `S4144` — Identical method implementations

**File:** `PatientPenalty.cs` (Domain/Entities)

```
Update this method so that its implementation is not identical to 'CreateAutomaticBlock'.
```

**Resolution:** `CreateManualBlock` intentionally duplicates `CreateAutomaticBlock` to preserve explicit domain intent. Each factory method represents a distinct business concept — merging them would sacrifice domain clarity for mechanical deduplication.

---

## Resolution Protocol for Failing Quality Gate

The SonarCloud Quality Gate is a **required check** — it cannot be overridden or bypassed. If it fails, the Pull Request cannot be merged. Period.

Resolution is handled through the SonarCloud web UI by inspecting the specific issues reported on the Pull Request.

### Genuine Issues — Fix in the PR Branch

If the reported issue is legitimate, it must be corrected with a commit on the same Pull Request branch:

- **Missing coverage:** Add unit tests that cover the new or modified business logic.
- **Code duplication:** Refactor the duplicated code to eliminate the structural repetition.
- **Bugs, vulnerabilities, security hotspots, or code smells:** Fix the issue in the source code.

### False Positives — Mark in SonarCloud UI

If the reported issue is a false positive, the resolution is managed entirely through the SonarCloud web UI — never by modifying source code to appease the tool.

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
New query or command handlers that follow established patterns may trigger the duplication threshold (≤ 3%). If the structural similarity is a natural consequence of the CQRS pattern and not actual copy-pasted business logic, the handler should be added to the duplication exclusions list.
