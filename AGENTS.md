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

**Status:** Under active development. Domain, Application, and Infrastructure layers are established. The Presentation (API) layer is not yet implemented.

---

## Architecture

### Layer Structure

```
ClinicFlow.Domain              → Pure domain: entities, value objects, enums, events, exceptions, services, policies
ClinicFlow.Domain.Tests        → Unit tests for the domain layer
ClinicFlow.Application         → CQRS orchestration: commands, queries, handlers, validators, behaviors
ClinicFlow.Application.Tests   → Unit tests for the application layer
ClinicFlow.Infrastructure      → Persistence (EF Core, configurations, migrations, seeding, repository implementations), Unit of Work
ClinicFlow.Infrastructure.Tests → Unit/integration tests for the infrastructure layer (persistence and seeding)
```

**Future layers** (not yet created):

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
) : IRequest;
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

### General Rules
See the [clinicflow-testing-base](.agents/skills/clinicflow-testing-base/SKILL.md) skill for general principles, naming conventions, and basic testing configurations.

### Domain Rules
See the [clinicflow-testing-domain](.agents/skills/clinicflow-testing-domain/SKILL.md) skill for rules regarding reflection for BaseEntity.Id, Value Object tests, and Domain Event assertions.

### Application Rules
See the [clinicflow-testing-application](.agents/skills/clinicflow-testing-application/SKILL.md) skill for rules regarding Callback Pattern, EntityNotFoundException, UnitOfWork verification, Create Handler Split, and Repository Write verification.

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
5. **Use `<inheritdoc/>`** on interface implementations instead of repeating docs.
6. **Use `<remarks>`** only for non-obvious business constraints or design decisions.
7. **Always write documentation in English**.

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

`Appointment`, `AppointmentTypeDefinition`, `ClinicalFormTemplate`, `Doctor`, `MedicalRecord`, `MedicalSpecialty`, `Patient`, `PatientPenalty`, `Schedule`, `User`

(Sub-entities under `Entities/ClinicalDetails/`: `DynamicClinicalDetail`)

### Value Objects

`AgeEligibilityPolicy`, `BloodType`, `ConsultationRoom`, `EmailAddress`, `EmergencyContact`, `MedicalLicenseNumber`, `PenaltyHistory`, `PersonName`, `PhoneNumber`, `TimeRange`

### Enums

`AppointmentCategory`, `AppointmentStatus`, `BlockDuration`, `PatientRelationship`, `PenaltyType`, `UserRole`

### Domain Events

`AppointmentCancelledEvent`, `AppointmentCheckedInEvent`, `AppointmentCompletedEvent`, `AppointmentLateCancelledEvent`, `AppointmentMarkedAsNoShowEvent`, `AppointmentRescheduledEvent`, `AppointmentScheduledEvent`, `AppointmentStartedEvent`, `MedicalRecordCreatedEvent`, `PatientReactivatedEvent`

### Domain Services & Policies

`AppointmentCancellationService`, `AppointmentReschedulingService`, `AppointmentSchedulingService`, `FamilyMemberRegistrationService`, `MedicalEncounterService`, `MetadataFormValidationPolicy`, `PatientPenaltyService`, `PrimaryProfileRegistrationService`, `WeeklyScheduleSetupService`

### Repository Interfaces

`IAppointmentRepository`, `IAppointmentTypeDefinitionRepository`, `IClinicalFormTemplateRepository`, `IDoctorRepository`, `IMedicalRecordRepository`, `IMedicalSpecialtyRepository`, `IPatientPenaltyRepository`, `IPatientRepository`, `IScheduleRepository`, `IUserRepository`

### External & Infrastructure Interfaces

`IJsonSchemaDefinitionValidator`, `IJsonSchemaValidator`, `IMedicalRecordValidationPolicy`, `IPhoneVerificationService`, `IUnitOfWork`
