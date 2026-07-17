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

When testing that a handler throws a **domain or validation exception** (one raised **before** any persistence call, e.g. not-found checks, business rule violations), **always** verify that `SaveChangesAsync` was never called. This ensures that a failed operation never persists partial state. If the handler also calls a repository write method (`CreateAsync`, `CreateRangeAsync`, etc.), **always** verify that it was never called as well; both verifications must appear together:

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

This rule applies to exception scenarios raised **before** the write path executes: `EntityNotFoundException`, `BusinessRuleValidationException`, domain-specific exceptions, etc. The repository write method verification is **mandatory** whenever the handler's code path includes a call to any persistence method (`CreateAsync`, `CreateRangeAsync`, `UpdateAsync`, etc.) that was never reached. Both `Times.Never` assertions must always appear as a pair.

## Create Handler Split

When a create handler returns a `Guid` and persists via a repository + `IUnitOfWork`, always split the happy path into **two separate tests**:

1. **Data validation test**: uses the `Callback` pattern to capture the entity and assert its properties:

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

2. **Persistence verification test**: verifies that both the repository and `SaveChangesAsync` were called exactly once:

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

**Happy path**: verify both were called exactly once:

```csharp
// Assert
_repositoryMock.Verify(
    x => x.CreateAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()),
    Times.Once
);
_unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
```

**Unhappy path**: verify neither was called:

```csharp
// Assert
_repositoryMock.Verify(
    x => x.CreateAsync(It.IsAny<Entity>(), It.IsAny<CancellationToken>()),
    Times.Never
);
_unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
```

This applies regardless of the specific write method: `CreateRangeAsync`, `UpdateAsync`, etc. The key principle: repository write verification and `SaveChangesAsync` verification are **inseparable pairs**. If one is present, the other must be too.

## UnitOfWork Verification on Success

For command handlers that modify an entity and persist via `IUnitOfWork` without returning a value (e.g., reactivate, deactivate, add/remove associations), the happy path test must **always** verify that `SaveChangesAsync` was called exactly once:

```csharp
// Assert
_unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

appointmentType.IsDeleted.Should().BeFalse();
```

This verification ensures the handler completes the full orchestration pipeline: fetch → delegate to domain → persist.

## Query Handler Verification

For query handlers, verify repository read methods in both happy and unhappy path tests:

**Happy path**: verify the read method was called exactly once:

```csharp
// Assert
_repositoryMock.Verify(
    x => x.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()),
    Times.Once
);
```

**Unhappy path (Exception)**: verify the read method was called exactly once to attempt fetching the entity, and verify that any other subsequent repository or service methods were never called:

```csharp
// Assert
await act.Should()
    .ThrowAsync<EntityNotFoundException>()
    .WithMessage(DomainErrors.General.NotFound);

_repositoryMock.Verify(
    x => x.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()),
    Times.Once
);

// If there are other repository or service dependencies:
_otherRepositoryMock.Verify(
    x => x.SomeMethodAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
    Times.Never
);
```

This ensures complete consistency across command and query tests, verifying that execution terminates early and cleanly on failures.

### Query Handler Return Verification

For query handlers, verify the returned results using structural comparison (`BeEquivalentTo`) instead of checking individual properties, following the specific pattern based on the return type. In all three patterns, omit `result.Should().NotBeNull()`: any subsequent property or collection access on `result` already throws if it were null, making the explicit check redundant.

#### Pattern A: Single DTO (e.g., GetById)

Construct an `expectedDto` matching the exact mapping performed by the handler (verify property names and any nested value objects against the handler's source code, do not assume), and assert the entire DTO:

```csharp
var expectedDto = new EntityDto(
    entity.Id,
    entity.Name,
    entity.Status
);

result.Should().BeEquivalentTo(expectedDto);
```

#### Pattern B: Simple Collection (e.g., IReadOnlyList<TDto>)

Map the mock entities to their corresponding DTOs using the same mapping as the handler, and assert the returned collection using `BeEquivalentTo`:

```csharp
var expectedDtos = entities.Select(e => new EntityDto(e.Id, e.Name));

result.Should().BeEquivalentTo(expectedDtos);
```

#### Pattern C: Paginated List (e.g., PaginatedList<TDto>)

Assert that the inner `Items` collection is structurally equivalent to the expected DTOs, and verify all pagination metadata fields (`TotalCount`, `PageNumber`, `TotalPages`) in both the happy path and the empty scenario:

```csharp
// Happy Path
var expectedDtos = entities.Select(e => new EntityDto(e.Id, e.Name));

result.Items.Should().BeEquivalentTo(expectedDtos);
result.TotalCount.Should().Be(2);
result.PageNumber.Should().Be(1);
result.TotalPages.Should().Be(1);

// Empty Path
result.Items.Should().BeEmpty();
result.TotalCount.Should().Be(0);
result.PageNumber.Should().Be(1); // use the PageNumber from that specific test's query, not always 1
result.TotalPages.Should().Be(0);
```

Do not use `WithStrictOrdering()`. The order of collections returned by repositories is not the handler's responsibility to validate (see `clinicflow-testing-repositories`).

> **Note:** Query handlers must never call `SaveChangesAsync`. If one does, that's a design smell; the handler belongs on the command side. No `SaveChangesAsync` verification is needed here precisely because it should never appear in a query handler.