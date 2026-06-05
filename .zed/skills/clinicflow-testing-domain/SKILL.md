---
name: clinicflow-testing-domain
description: Use this skill alongside clinicflow-testing-base when writing tests for the ClinicFlow Domain layer. Covers reflection for BaseEntity.Id, Value Object test structure, and Domain Event assertions.
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

## Domain Event Assertions

When verifying that a specific domain event was emitted and is the only one of its type in the collection, standardize on the following pattern using `OfType<T>()` and `ContainSingle()`:

```csharp
entity.DomainEvents.OfType<XEvent>().Should().ContainSingle();
```
