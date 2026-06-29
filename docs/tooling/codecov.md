# Codecov Guidelines and Conventions

This document outlines the design decisions, conventions, and guidelines for using **Codecov** within the ClinicFlow project.

For the exact configuration, refer to the [codecov.yml](../../codecov.yml) file.

---

## Design Decisions & Philosophy

### 1. Pure Domain and Application Code
In ClinicFlow, we strictly adhere to clean code and Domain-Driven Design (DDD) principles. The Domain layer must remain pure and free from tooling- or infrastructure-specific attributes. 

For this reason, **we do not use C# attributes such as `[ExcludeFromCodeCoverage]`** to silence coverage tools. The codebase must speak its own language, and we prioritize code purity over achieving a perfect 100% green status on automated reports if it requires adding non-domain clutter.

### 2. Patch Coverage Target
The target patch coverage is configured to **80%** (see `codecov.yml`). This threshold ensures that new logic and modifications introduced in Pull Requests are adequately covered by unit tests, preventing regressions and maintaining high software quality.

---

## Configured Exclusions

Certain file patterns are globally ignored by Codecov because they do not contain testable business logic. These are configured in the `ignore` section of `codecov.yml`:

- **Migrations (`**/Migrations/**`):** Entity Framework Core migration history.
- **Designer files (`**/*.Designer.cs`):** Automatically generated UI or resource code.
- **Tests (`tests/**/*`):** The test projects themselves.
- **Domain Events (`**/Events/**`):** Simple data-carrying records representing events, which have no business logic and are verified indirectly through the entities that raise them.
- **Dependency Injection Registrations (`ClinicFlow.Infrastructure/DependencyInjection.cs`, `ClinicFlow.Application/DependencyInjection.cs`):** Boilerplate wiring files that register dependencies in the service container and contain no business logic.

---

## Criteria for Ignoring Failed Checks in Pull Requests (False Positives)

Since we do not use attributes to exclude code from coverage, Codecov's patch coverage check might occasionally fail (fall below 80%) on Pull Requests that modify lines of code containing no testable logic. 

In these specific scenarios, **reviewers and authors are permitted and expected to ignore the red Codecov check and merge the PR**.

### Permissible False-Positive Scenarios

#### EF Core Parameterless Constructors
Entities often require a parameterless constructor (usually `private` or `protected`) for EF Core materialization. These constructors contain no business logic:

```csharp
// Example: Private constructor for ORM compatibility
private Appointment() => TimeRange = null!;
```

When refactoring or touching these constructors (e.g., converting them from `{}` to expression-bodied `=>` syntax), Codecov marks the changed lines as "missing coverage" because they are not invoked in tests. If a PR contains changes to multiple entities' parameterless constructors, it will likely fail the patch coverage target.

#### Application Data Transfer Objects (DTOs)
```csharp
public sealed record CancelAppointmentByDoctorCommand(
    Guid AppointmentId,
    Guid InitiatorUserId,
    string? Reason
) : IRequest;
```

These objects contain no behavior or business logic. Writing unit tests for them is unnecessary. If they are modified or introduced in a PR without being explicitly instantiated in a unit test, Codecov will report them as missing coverage.

### Reviewer Protocol
Before merging a PR with a failing Codecov check, the author and reviewer must verify that any missing lines highlighted in the Codecov report belong exclusively to the false-positive categories described above, and that all actual business logic has appropriate test coverage meeting project standards.

If these conditions are met, reviewers may override the failing Codecov check only after explicit verification and approval, confirming that no business logic is affected.