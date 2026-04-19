# ClinicFlow — AI Agent Context

> This document is the single source of truth for any AI assistant working on this codebase.
> Read it completely before writing any code. Violating these rules is unacceptable.

## Project Overview

**ClinicFlow** is a clinical management and appointment scheduling system built with **.NET / C#** following strict **Domain-Driven Design (DDD)** and **CQRS** (Command Query Responsibility Segregation) principles.

The system handles:

- Management of doctors, medical specialties, and schedules.
- Appointment scheduling, rescheduling, cancellation, check-in, start, completion, and no-show marking — all with strict business rules per actor role (Doctor, Patient, Staff).
- Patient management, family members, medical profiles, and penalty/blocking systems for non-compliance.
- Medical records with clinical forms, templates, and JSON schema validation.

**Status:** Under active development. Domain and Application layers are established. Infrastructure and Presentation (API) layers are not yet implemented.

---

## Architecture

### Layer Structure

```
ClinicFlow.Domain              → Pure domain: entities, value objects, enums, events, exceptions, services, policies
ClinicFlow.Domain.Tests        → Unit tests for the domain layer
ClinicFlow.Application         → CQRS orchestration: commands, queries, handlers, validators, behaviors
ClinicFlow.Application.Tests   → Unit tests for the application layer
```

**Future layers** (not yet created):

- `ClinicFlow.Infrastructure` — Persistence (EF Core / Dapper), external services
- `ClinicFlow.API` — ASP.NET Core Web API / Minimal APIs

### Technology Stack

| Layer | Libraries |
|---|---|
| **Domain** | Zero external packages. No NuGet dependencies allowed. |
| **Application** | MediatR 14.0, FluentValidation 12.1, Microsoft.Extensions.DependencyInjection.Abstractions |
| **Tests (Domain)** | xUnit 2.9, FluentAssertions 8.8, Microsoft.Extensions.TimeProvider.Testing 10.4 |
| **Tests (Application)** | xUnit 2.9, FluentAssertions 8.8, Moq 4.20, Microsoft.Extensions.TimeProvider.Testing 10.4, FluentValidation.TestHelper |

### Key Dependency Flow

```
Domain ← Application ← Infrastructure ← API
   ↑          ↑              ↑
   └─ Zero    └─ MediatR     └─ EF Core (future)
     packages   FluentVal
```

---

## Domain Layer Rules

### Absolute Prohibitions

1. **No external NuGet packages** — The domain layer must have zero `PackageReference` entries.
2. **No external namespaces** — No `using` statements referencing anything outside `ClinicFlow.Domain`.
3. **No infrastructure queries** — Domain services never inject repositories or make database calls. They receive pre-fetched data from the Application layer.
4. **No interfaces injected** — Domain services must not depend on injected interfaces. The single exception is infrastructure validation policies (see [Policy Pattern](#policy-pattern)).

### Entity Design

- All entities inherit from `BaseEntity` which provides `Id` (Guid, private set), `IsDeleted` (soft-delete), and `DomainEvents`.
- Entities use the **Factory Method** pattern — construction is done through static `Create(...)` or `Schedule(...)` methods, never through direct `new` (constructors are parameterless and protected/private for ORM compatibility).
- Entity methods that **require a Domain Service** to function must be `internal`. Methods that work independently are `public`. This visibility convention communicates architectural intent.

### Domain Errors

All error messages are standardized constants in `DomainErrors` (`ClinicFlow.Domain/Common/DomainErrors.cs`), organized by subdomain:

```csharp
DomainErrors.General.NotFound          // "ENTITY_NOT_FOUND"
DomainErrors.Appointment.CannotCancel  // "CANCELLATION_NOT_ALLOWED"
DomainErrors.Validation.ValueRequired  // "VALUE_REQUIRED"
```

New exceptions must reference existing constants or add new ones to `DomainErrors`. Never use hardcoded error strings.

### Exception Hierarchy

```
DomainException (abstract base)
├── BusinessRuleValidationException  → business rule violations
├── DomainValidationException        → input/state validation failures
├── EntityNotFoundException          → entity not found (includes EntityName + Id)
└── Specific exceptions (in Exceptions/<Subdomain>/)
    ├── AppointmentCancellationNotAllowedException
    ├── AppointmentCancellationUnauthorizedException
    ├── AppointmentConflictException
    ├── PatientBlockedException
    ├── DoctorNotAvailableException
    └── ... (each under its subdomain folder)
```

### Context and Args Pattern

Domain services use a **Context + Args** pattern to separate state from intent:

**Context** = what exists in the world for validation (one per operation, role-agnostic):

```csharp
// Services/Contexts/AppointmentReschedulingContext.cs
public sealed record class AppointmentReschedulingContext
{
    public IReadOnlyList<PatientPenalty> Penalties { get; init; } = [];
    public Schedule? DoctorSchedule { get; init; }
    public bool HasConflict { get; init; }
}
```

**Args** = what the actor wants to do (one per role, encapsulates actor-specific intent):

```csharp
// Services/Args/Cancellation/DoctorCancellationArgs.cs
public sealed record DoctorCancellationArgs
{
    public Doctor? InitiatorDoctor { get; init; }
    public required MedicalSpecialty Specialty { get; init; }
    public string? Reason { get; init; }
    public DateTime CancelledAt { get; init; }
}
```

**When to use Args:** If a method would have 4 or more parameters, encapsulate them in an Args record. Each role (Doctor, Patient, Staff) gets its own Args reflecting that role's business rules.

### Domain Events

- Events use past-tense naming: `AppointmentCancelledEvent`, `AppointmentScheduledEvent`, `AppointmentMarkedAsNoShowEvent`.
- Events are raised via `entity.AddDomainEvent(new XxxEvent(...))` inside domain logic.

### Policy Pattern

The only exception to the "no interfaces in domain" rule is when validation requires infrastructure (e.g., JSON schema validation). This is done through policies:

```csharp
// IJsonSchemaValidator is an interface defined in the Domain layer,
// implemented in Infrastructure, injected via DI into the Policy class.
public class MetadataFormValidationPolicy(IJsonSchemaValidator jsonSchemaValidator)
    : IMedicalRecordValidationPolicy { ... }
```

---

## Application Layer Rules

### Commands and Queries

- Every Command/Query must be `sealed record` with a primary constructor:

```csharp
public sealed record CancelAppointmentByDoctorCommand(
    Guid AppointmentId,
    Guid InitiatorUserId,
    string? Reason
) : IRequest, ICancelCommand;
```

- Queries that return data use `IRequest<TResult>`.

### Validators

- Every Command/Query with **at least 1 parameter** must have a corresponding `Validator` class using FluentValidation.
- Commands/Queries with **no parameters** do not need a validator.

```
// Has parameters → needs validator
GetPenaltiesByPatientIdQuery(Guid PatientId) → GetPenaltiesByPatientIdQueryValidator ✓

// No parameters → no validator
GetActiveBlockedPatientsQuery() → No validator needed ✓
```

### Shared Validators

When two or more Commands have **nearly identical or identical input** (e.g., `CancelByDoctor`, `CancelByPatient`, `CancelByStaff`), extract shared validation into:

1. An interface: `ICancelCommand` in `Commands/Shared/Cancel/`
2. A base validator: `CancelCommandValidatorBase<T>` in `Commands/Shared/Cancel/`
3. Concrete validators extend the base and add role-specific rules if needed.

The shared base is tested once in `Tests/.../Shared/Cancel/CancelCommandValidatorBaseTests.cs`. Each concrete validator has its own test file that tests **only** the happy path + its specific failure cases — never re-tests shared rules.

### Handlers

- Every Handler must be `sealed class` with a primary constructor:

```csharp
public sealed class CancelAppointmentByDoctorCommandHandler(
    TimeProvider timeProvider,
    IAppointmentRepository appointmentRepository,
    IDoctorRepository doctorRepository,
    IMedicalSpecialtyRepository specialtyRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CancelAppointmentByDoctorCommand> { ... }
```

### Null-Check Pattern for Entity Fetching

When fetching an entity that business logic requires, always use this pattern:

```csharp
var appointment =
    await appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken)
    ?? throw new EntityNotFoundException(
        DomainErrors.General.NotFound,
        nameof(Appointment),
        request.AppointmentId
    );
```

### Conditional Logic Prohibition

**No `if` statements or conditional logic in the Application layer.** If there is decision logic, it belongs in the Domain layer. The Application layer only orchestrates: fetch → delegate to domain → persist.

### Time Abstraction

Always use `TimeProvider` (abstract class) instead of `DateTime.UtcNow` or `DateTimeOffset.UtcNow`. Inject `TimeProvider` via the primary constructor and call `timeProvider.GetUtcNow().UtcDateTime`.

---

## Testing Rules

### General Principles

1. **Every feature must have tests** — every new entity, value object, domain service, command, query, handler, and validator must be accompanied by its corresponding test file.
2. **No conditional logic in tests** — no `if`, `switch`, ternary operators, or any decision logic inside a test. If you need different scenarios, write separate `[Fact]` methods.
3. **No helper methods with logic** — test helpers must only construct objects. No conditionals, no decisions. Just build and return:

```csharp
// ✅ Good — pure construction
private static TimeRange CreateTimeRange(int startHour, int endHour) =>
    TimeRange.Create(TimeSpan.FromHours(startHour), TimeSpan.FromHours(endHour));

// ❌ Forbidden — conditional logic in helper
private static Patient CreatePatient(bool isPremium)
{
    if (isPremium) return Patient.CreatePremium(...);
    return Patient.CreateRegular(...);
}
```

4. **Expression-body (`=>`) for single-return helpers** — if the helper just returns the constructed object, use expression-body syntax.
5. **AAA Comments** — always comment `// Arrange`, `// Act`, `// Assert` (or `// Arrange & Act`, `// Act && Assert` when combined).

### Test Naming Convention

```
MethodName_ShouldExpectedBehavior
MethodName_ShouldExpectedBehavior_WhenCondition
```

### Framework and Libraries

| Library | Usage |
|---|---|
| **xUnit** | `[Fact]` for single scenarios, `[Theory]` + `[InlineData]` for parameterized tests |
| **FluentAssertions** | All assertions — `.Should().Be()`, `.Should().Throw<>()`, `.Should().ContainSingle()` etc. |
| **Moq** | Mocking repository interfaces in Application layer tests |
| **FakeTimeProvider** | Always use `private readonly FakeTimeProvider _fakeTime = new();` for time-dependent tests |
| **FluentValidation.TestHelper** | Testing validators — `_sut.TestValidate(command).ShouldNotHaveAnyValidationErrors()` |

### `_sut` Convention

The system under test is always named `_sut`:

```csharp
private readonly BlockPatientCommandValidator _sut;
private readonly CreateCompletePatientProfileCommandHandler _sut;
```

### Builder Pattern for Tests

When an object's construction varies significantly across tests, use a private nested `Builder` class:

```csharp
private class AppointmentBuilder
{
    private Guid _patientId = Guid.NewGuid();
    private Guid _doctorId = Guid.NewGuid();
    private DateTime _scheduledDateTime;

    public AppointmentBuilder WithPatientId(Guid patientId)
    {
        _patientId = patientId;
        return this;
    }

    public Appointment Build() =>
        Appointment.Schedule(_patientId, _doctorId, ...);
}
```

### Reflection for BaseEntity.Id

When a test needs to control an entity's `Id` (which has `private set`), use the shared extension:

```csharp
using ClinicFlow.Domain.Tests.Shared;

var doctor = Doctor.Create(...);
doctor.SetId(specificGuidNeededForTest);
```

This is in `EntityTestExtensions.cs` and uses reflection. Only use it when the test genuinely requires a specific Id.

### String Validation Tests

Tests that validate string input (null, empty, whitespace) must always be `[Theory]` with `[InlineData]`:

```csharp
[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData("   ")]
public void Create_ShouldThrowException_WhenValueIsEmpty(string? value) { ... }
```

### Value Object Tests

For value objects with computed/getter properties, the `// Act` section may be omitted — construction is the act:

```csharp
[Fact]
public void HasPriorWarnings_ShouldReturnTrue_WhenWarningsExist()
{
    // Arrange
    var history = new PenaltyHistory([...]);

    // Assert
    history.HasPriorWarnings.Should().BeTrue();
}
```

### Application Handler Tests — Callback Pattern

When a handler's return is not explicit (e.g., it returns the Id from the repository `AddAsync`), use Moq's `.Callback` to capture the entity:

```csharp
Patient? capturedPatient = null;
_patientRepositoryMock
    .Setup(x => x.CreateAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()))
    .Callback<Patient, CancellationToken>((p, _) => capturedPatient = p);

// Act
var result = await _sut.Handle(command, CancellationToken.None);

// Assert
capturedPatient.Should().NotBeNull();
capturedPatient!.UserId.Should().Be(command.UserId);
```

### Application Handler Tests — EntityNotFoundException

Always assert both the message and the `EntityName`:

```csharp
var exceptionAssertion = await act.Should()
    .ThrowAsync<EntityNotFoundException>()
    .WithMessage(DomainErrors.General.NotFound);
exceptionAssertion.Which.EntityName.Should().Be(nameof(Appointment));
```

---

## Code Style

### Language

- All code (classes, methods, variables, namespaces, comments, XML docs) must be in **English**.
- Chat/discussions with the developer can be in **Spanish**.
- The developer handles commits manually — never auto-commit.

### C# Preferences

- Use `sealed` on all classes that are not designed for inheritance (handlers, commands, args, contexts).
- Use `record` for immutable data structures (commands, queries, args, contexts).
- Use primary constructors for dependency injection in handlers and for data in commands.
- Nullable reference types are active — respect nullability annotations.

### XML Documentation Rules

1. **Only document when it adds value** — do not document self-explanatory members.
2. **Never document test classes** — no XML docs in `*Tests.cs`, `*Fixture.cs`, or any test project file.
3. **Start `<summary>` with a third-person verb**: "Marks the appointment as...", "Validates the patient's..."
4. **Do not repeat the member name** in the documentation.
5. **Use `<exception cref="...">` only for domain exceptions** the caller must handle.
6. **Use `<inheritdoc/>`** on interface implementations instead of repeating docs.
7. **Use `<remarks>`** only for non-obvious business constraints or design decisions.
8. **Always write documentation in English**.

```csharp
// ❌ No value added — name says it all
/// <summary>
/// Gets the patient name.
/// </summary>
public string Name { get; set; }

// ✅ Adds value — explains non-obvious constraint
/// <summary>
/// Marks the appointment as a no-show. Can only be executed if the appointment is in a confirmed state.
/// </summary>
/// <exception cref="DomainException">If the appointment is not in a confirmed state.</exception>
internal void MarkAsNoShow() { }
```

---

## Interaction Rules with Developer

### Mandatory — Ask Before Acting

1. **Before installing any NuGet package** in any layer (including test projects) — propose it, explain why, and wait for approval.
2. **Before modifying significant domain business logic** — explain the proposed change, the reasoning, and wait for discussion.

### Prohibited Actions

- Never install packages in `ClinicFlow.Domain`.
- Never add conditional/decision logic in the Application layer handlers.
- Never create test helpers with conditional logic.
- Never use `DateTime.UtcNow` / `DateTimeOffset.UtcNow` directly — always use `TimeProvider`.
- Never skip writing tests for new features.
- Never auto-commit or auto-push.

---

## Domain Model Reference

### Entities

`Appointment`, `AppointmentTypeDefinition`, `ClinicalFormTemplate`, `Doctor`, `MedicalRecord`, `MedicalSpecialty`, `Patient`, `PatientPenalty`, `Schedule`

(Sub-entities under `Entities/ClinicalDetails/`: `DynamicClinicalDetail`)

### Value Objects

`AgeEligibilityPolicy`, `BloodType`, `EmailAddress`, `EmergencyContact`, `MedicalLicenseNumber`, `PenaltyHistory`, `PersonName`, `PhoneNumber`, `TimeRange`

### Enums

`AppointmentCategory`, `AppointmentStatus`, `BlockDuration`, `DayOfWeek`, `PatientRelationship`, `PenaltyType`, `UserRole`

### Domain Events

`AppointmentCancelledEvent`, `AppointmentCheckedInEvent`, `AppointmentCompletedEvent`, `AppointmentMarkedAsNoShowEvent`, `AppointmentRescheduledEvent`, `AppointmentScheduledEvent`, `AppointmentStartedEvent`, `MedicalRecordCreatedEvent`

### Domain Services

`AppointmentCancellationService`, `AppointmentReschedulingService`, `AppointmentSchedulingService`, `MedicalEncounterService`, `PatientPenaltyService`

### Repository Interfaces

`IAppointmentRepository`, `IAppointmentTypeDefinitionRepository`, `IClinicalFormTemplateRepository`, `IDoctorRepository`, `IMedicalRecordRepository`, `IMedicalSpecialtyRepository`, `IPatientPenaltyRepository`, `IPatientRepository`, `IScheduleRepository`, `IUnitOfWork`
