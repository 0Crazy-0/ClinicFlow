---
name: clinicflow-testing-domain
description: Use this skill alongside clinicflow-testing-base when writing tests for the ClinicFlow Domain layer. Covers reflection for BaseEntity.Id, Value Object test structure, Domain Event assertions, and exception classification for null guards.
---

# ClinicFlow Testing Domain

Specific rules and conventions for testing the Domain layer in ClinicFlow.

## Reflection for BaseEntity.Id

When a test needs to control an entity's `Id` (which has `private set`), use the shared extension:

```csharp
using ClinicFlow.Domain.Tests.Shared;

var doctor = Doctor.Create(...);
doctor.SetId(specificGuidNeededForTest);
```

This is in `EntityTestExtensions.cs` and uses reflection. Only use it when the test genuinely requires a specific Id.

## Value Object Tests

For value objects with computed/getter properties, the `// Act` section may be omitted; construction is the act:

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

## Domain Event Assertions

When verifying that a specific domain event was emitted and is the only one of its type in the collection, standardize on the following pattern using `OfType<T>()` and `ContainSingle()`:

```csharp
entity.DomainEvents.OfType<XEvent>().Should().ContainSingle();
```

## Exception Classification in Null Guards

Domain services distinguish two categories of null guards. Tests must assert the correct exception type for each:

1. **Contract nulls** (plumbing parameters: entities, Args, Contexts, schedules, and their critical `required` object members such as `args.TargetPatient` or `context.Specialty`). A null here is a caller bug, not a business rule violation. The service uses `ArgumentNullException.ThrowIfNull(...)` and the test expects `ArgumentNullException`:

```csharp
[Fact]
public void CancelByStaff_ShouldThrowArgumentNullException_WhenReasonIsNull()
{
    // Arrange
    var appointment = CreateAppointment();
    var args = new StaffCancellationArgs
    {
        InitiatorUserId = Guid.CreateVersion7(),
        Reason = null!,
        CancelledAt = _fakeTime.GetUtcNow().UtcDateTime,
    };

    // Act
    var act = () => AppointmentCancellationService.CancelByStaff(appointment, args);

    // Assert
    act.Should().Throw<ArgumentNullException>();
}
```

Do not assert `WithParameterName(...)`; a plain exception-type assertion is the standard.

2. **Business rule nulls** (currently only `SchedulingClearance` in scheduling and rescheduling methods). A null clearance means the regional scheduling regulation was not enforced, which violates a domain rule. The service throws `BusinessRuleValidationException(DomainErrors.Scheduling.MissingClearance)`:

```csharp
[Fact]
public void ScheduleByPatient_ShouldThrowBusinessRuleValidationException_WhenClearanceIsNull()
{
    // Arrange & Act
    var act = () =>
        AppointmentSchedulingService.ScheduleByPatient(
            CreateAppointmentType(),
            CreateValidPatientSchedulingArgs(),
            new PatientSchedulingContext { DoctorSchedule = CreateSchedule() },
            null!
        );

    // Assert
    act.Should()
        .Throw<BusinessRuleValidationException>()
        .WithMessage(DomainErrors.Scheduling.MissingClearance);
}
```

### Scope of `ArgumentNullException.ThrowIfNull`

Only apply `ArgumentNullException.ThrowIfNull` to `required` object references (parameters and their `required` members), which are impossible to omit at compile time but could still be passed as `null` deliberately. Do **not** apply it to optional or non-`required` properties (primitives with default values, nullable strings, etc.); those states are validated by the domain's own business rules and must keep throwing the corresponding `DomainException` type.