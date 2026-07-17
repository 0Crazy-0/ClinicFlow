---
name: clinicflow-testing-base
description: Use this skill before writing any test file in the ClinicFlow project. Covers general principles, naming conventions, _sut convention, AAA structure, string validation, and JSON literals. Always read this alongside the layer-specific testing skill (domain, application, or infrastructure).
---

# ClinicFlow Testing Base

General principles, naming conventions, and coding style rules that every unit test must follow.

## General Principles

1. **Every feature must have tests**: every new entity, value object, domain service, command, query, handler, and validator must be accompanied by its corresponding test file.
2. **No conditional logic in tests**: no `if`, `switch`, ternary operators, or any decision logic inside a test. If you need different scenarios, write separate `[Fact]` methods.
3. **No helper methods with logic**: test helpers must only construct objects. No conditionals, no decisions. Just build and return:

```csharp
// ✅ Good: pure construction
private static TimeRange CreateTimeRange(int startHour, int endHour) =>
    TimeRange.Create(TimeSpan.FromHours(startHour), TimeSpan.FromHours(endHour));

// ❌ Forbidden: conditional logic in helper
private static Patient CreatePatient(bool isPremium)
{
    if (isPremium) return Patient.CreatePremium(...);
    return Patient.CreateRegular(...);
}
```

4. **Expression-body (`=>`) for single-return helpers**: if the helper just returns the constructed object, use expression-body syntax.
5. **AAA Comments**: always comment `// Arrange`, `// Act`, `// Assert` (or `// Arrange & Act`, `// Act && Assert` when combined).

## Test Naming Convention

```
MethodName_ShouldExpectedBehavior
MethodName_ShouldExpectedBehavior_WhenCondition
```

## `_sut` Convention

The system under test is always named `_sut`:

```csharp
private readonly BlockPatientCommandValidator _sut;
private readonly CreateCompletePatientProfileCommandHandler _sut;
```

## Framework and Libraries

| Library | Usage |
|---|---|
| **xUnit** | `[Fact]` for single scenarios, `[Theory]` + `[InlineData]` for parameterized tests |
| **FluentAssertions** | All assertions: `.Should().Be()`, `.Should().Throw<>()`, `.Should().ContainSingle()` etc. |
| **FakeTimeProvider** | Always use `private readonly FakeTimeProvider _fakeTime = new();` for time-dependent tests |

## String Validation Tests

Tests that validate string input (null, empty, whitespace) must always be `[Theory]` with `[InlineData]`:

```csharp
[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData("   ")]
public void Create_ShouldThrowException_WhenValueIsEmpty(string? value) { ... }
```

## JSON String Literals in Tests

When defining JSON strings in tests (e.g., for JSON schema validation or mocking serialized data), always use **C# Raw String Literals** (`"""..."""`). This avoids escaping quotes and improves readability.

```csharp
// ✅ Good: raw string literal
var schemaDefinition = """{"type":"object","properties":{"bp":{"type":"string"}}}""";
var detail = new StubClinicalDetail("VITALS", """{"bp":"120/80"}""");

// ❌ Forbidden: escaped quotes
var schemaDefinition = "{\"type\":\"object\",\"properties\":{\"bp\":{\"type\":\"string\"}}}";
```
