---
name: clinicflow-testing-application
description: Use this skill alongside clinicflow-testing-base when writing tests for the ClinicFlow Application layer. Covers Callback Pattern, EntityNotFoundException assertions, UnitOfWork verification, Create Handler Split, and Repository Write verification.
---

# ClinicFlow Testing Application

Specific rules and conventions for testing the Application layer in ClinicFlow.

## Callback Pattern

When a handler's return is not explicit (e.g., it returns the Id from the repository `AddAsync`), use Moq's `.Callback` to capture the entity:

```csharp
Patient? capturedPatient = null;
_patientRepositoryMock
    .Setup(x => x.CreateAsync(It.IsAny<Patient>(), It.IsAny<CancellationToken>()))
    .Callback<Patient, CancellationToken>((p, _) => capturedPatient = p);

// Act
var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

// Assert
capturedPatient.Should().NotBeNull();
capturedPatient.UserId.Should().Be(command.UserId);
```

## EntityNotFoundException

Always assert both the message and the `EntityName`:

```csharp
var exceptionAssertion = await act.Should()
    .ThrowAsync<EntityNotFoundException>()
    .WithMessage(DomainErrors.General.NotFound);
exceptionAssertion.Which.EntityName.Should().Be(nameof(Appointment));
```

## UnitOfWork Verification on Exceptions

When testing that a handler throws an exception, **always** verify that `SaveChangesAsync` was never called. This ensures that a failed operation never persists partial state. If the handler also calls a repository write method (`CreateAsync`, `CreateRangeAsync`, etc.), **always** verify that it was never called as well — both verifications must appear together:

```csharp
// Assert
await act.Should()
    .ThrowAsync<EntityNotFoundException>()
    .WithMessage(DomainErrors.General.NotFound);

_repositoryMock.Verify(
    x => x.CreateAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()),
    Times.Never
);
_unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
```

This rule applies to **every** exception scenario — `EntityNotFoundException`, `BusinessRuleValidationException`, domain-specific exceptions, etc. The repository write method verification is **mandatory** whenever the handler's code path includes a call to any persistence method (`CreateAsync`, `CreateRangeAsync`, `UpdateAsync`, etc.). Both `Times.Never` assertions must always appear as a pair.

## Create Handler Split

When a create handler returns a `Guid` and persists via a repository + `IUnitOfWork`, always split the happy path into **two separate tests**:

1. **Data validation test** — uses the `Callback` pattern to capture the entity and assert its properties:

```csharp
[Fact]
public async Task Handle_ShouldCreateEntity_WhenValidCommand()
{
    // Arrange
    Entity? capturedEntity = null;
    _repositoryMock
        .Setup(x => x.CreateAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()))
        .Callback<Entity, CancellationToken>((e, _) => capturedEntity = e);

    // Act
    var result = await _sut.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    result.Should().NotBeEmpty();
    capturedEntity.Should().NotBeNull();
    capturedEntity.Name.Should().Be(command.Name);
    // ... assert all mapped properties
}
```

2. **Persistence verification test** — verifies that both the repository and `SaveChangesAsync` were called exactly once:

```csharp
[Fact]
public async Task Handle_ShouldCallRepositoryCreateAndSaveChanges_WhenValidCommand()
{
    // Arrange
    _repositoryMock
        .Setup(x => x.CreateAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((Entity e, CancellationToken _) => e);

    // Act
    await _sut.Handle(command, TestContext.Current.CancellationToken);

    // Assert
    _repositoryMock.Verify(
        x => x.CreateAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()),
        Times.Once
    );
    _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
}
```

This separation keeps each test focused on a single concern: one validates correctness of the created entity, the other validates the persistence pipeline.

## Repository Write Method Verification

Any handler that calls a repository write method (`CreateAsync`, `CreateRangeAsync`, `UpdateAsync`, etc.) must verify that method in **both** happy and unhappy path tests, always paired with the `SaveChangesAsync` verification:

**Happy path** — verify both were called exactly once:

```csharp
// Assert
_repositoryMock.Verify(
    x => x.CreateAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()),
    Times.Once
);
_unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
```

**Unhappy path** — verify neither was called:

```csharp
// Assert
_repositoryMock.Verify(
    x => x.CreateAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()),
    Times.Never
);
_unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
```

This applies regardless of the specific write method — `CreateRangeAsync`, `UpdateAsync`, etc. The key principle: repository write verification and `SaveChangesAsync` verification are **inseparable pairs**. If one is present, the other must be too.

## UnitOfWork Verification on Success

For command handlers that modify an entity and persist via `IUnitOfWork` without returning a value (e.g., reactivate, deactivate, add/remove associations), the happy path test must **always** verify that `SaveChangesAsync` was called exactly once:

```csharp
// Assert
_unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

appointmentType.IsDeleted.Should().BeFalse();
```

This verification ensures the handler completes the full orchestration pipeline: fetch → delegate to domain → persist.
