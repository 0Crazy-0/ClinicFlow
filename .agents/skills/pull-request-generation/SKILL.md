---
name: pull-request-generation
description: Generate standardized pull request descriptions following Conventional Commits format. Use whenever the developer explicitly asks for a PR description, branch name, or PR title. Enforces paragraph-style summaries, proper scoping, correct type-of-change selection, and test-class-level verification instructions. Never generate a PR unless explicitly asked.
---

# Pull Request Generation

## When to Use

Generate the PR description **only when the developer explicitly asks for it**. There must be sufficient context from the preceding conversation to fill every section accurately. If there is not enough context, say so; do not fabricate details.

When generating the PR, also suggest a **branch name** following the same Conventional Commits prefix (e.g., `feat/add-appointment-type-crud`, `test/refine-handler-assertions`).

## PR Template

Use the following template:

```markdown
## Summary
<!-- What changed and why, in a few lines -->

## Type of change
- [ ] Bug fix
- [ ] New feature
- [ ] Refactor
- [ ] Performance
- [ ] Test / QA
- [ ] Documentation
- [ ] Tooling
- [ ] Style

## How to test
<!-- Steps to verify this works as expected -->

## Checklist
- [ ] Code follows project standards
- [ ] Tests pass
- [ ] No Console.WriteLine or dead code
```

## PR Title

Titles follow **Conventional Commits** format in lowercase:

```
<type>(<optional scope>): <short description>
```

**Types:** `feat`, `fix`, `refactor`, `test`, `chore`, `docs`, `style`.

**Scope rules:**

- The scope is optional. Use it only when the PR is clearly focused on a specific area and the scope adds value.
- Predefined scopes: `appointments`, `appointment-type`, `clinical-forms`, `agents`, `penalty`, `schedule`, `doctor`, `github`, `medical-records`, `patients`, `medical-specialty`.
- If the changes touch an area not covered by the predefined scopes, suggest a new scope that is concise and meaningful.
- If the PR is test-related (adding, refactoring, or fixing tests), the title **must** start with `test`, never `refactor(test)`.

Examples:

```
feat: add appointment type CRUD commands
chore(agents): configure AI context, rules, and tooling
test: correct test phase comments
test(patient): use factory over reflection
test(appointments): refactor creation helpers to remove flag argument
docs: update README with setup instructions
```

**Forbidden formats**: never use branch-style casing:

```
❌ Feat/appointment context objects
❌ Test/medical encounter domain validation
```

## Summary Rules

1. **Paragraph format only**: write a cohesive paragraph. Never use bullet points (`-`) or numbered lists to describe the changes.

2. **Code references in backticks**: wrap class names, exceptions, files, properties, and any code-related term in single backticks: `` `ClinicalFormTemplate` ``, `` `EntityNotFoundException` ``.

3. **No technology or pattern name-dropping**: do not mention MediatR, CQRS, FluentValidation, DDD, or similar. Use natural terms: "commands" instead of "CQRS commands", "validators" instead of "FluentValidation validators", "handlers" instead of "MediatR handlers".

4. **Domain rules are concrete, not examples**: never use "e.g." or parenthetical asides to describe domain constraints. State them directly as facts.

Good:

```
Introduces the complete command lifecycle (Create, Update, Delete) for `ClinicalFormTemplate` entities. This implementation extends the domain repository contracts, introduces a JSON schema validation policy, and establishes the necessary application handlers and validation rules to support the robust management of dynamic clinical forms.
```

```
Added comprehensive unit tests for `AppointmentSchedulingService` and `AppointmentReschedulingService`. These test suites ensure that the core domain logic for handling appointments works perfectly, specifically covering authorization guards, doctor availability enforcement, and schedule conflict detection. The tests also comprehensively validate other essential business rules, including incomplete profiles, data mismatches, age eligibility, patient penalty blocking, and overbooking bypass conditions for staff and doctors.
```

Bad: uses bullets instead of paragraph:

```text
❌ This PR introduces the medical records feature. It adds:
- CompleteMedicalEncounterCommand and handler...
- Queries to fetch medical records...
```

Bad: treats domain rules as optional examples:

```
❌ ...while enforcing domain rules (e.g., code immutability and null-fallback for schemas).
```

## Type of Change Rules

- **Bug fix**: Select when the PR fixes incorrect behavior.
- **New feature**: Select this alone when delivering a feature, even if it includes tests and XML documentation; those are expected to accompany any feature.
- **Refactor**: Select for structural improvements that do not change behavior.
- **Performance**: Select for changes that improve execution speed, memory footprint, or resource utilization.
- **Test / QA**: Select when the PR exclusively adds, refactors, or modifies tests. If tests are refactored without adding new coverage, combine with `Refactor`.
- **Documentation**: Select only for XML documentation additions or official documentation files (README, guides). Inline `//` comments do not count.
- **Tooling**: Select for CI/CD, build setups, dependency updates, or tooling configurations.
- **Style**: Select for formatting, linting, or non-functional styling adjustments.

Mark only what applies; do not over-select.

## How to Test Rules

1. **Be direct**: list the specific test classes that verify the changes. Do not describe what to "test manually" or say "run `dotnet test`".

2. **Tie tests to the change (when applicable)**: when the PR refactors production code or existing test code, each affected test class description must explain what it verifies *in relation to the specific change made*, not just describe the class's general purpose. E.g. instead of "Verifies the cancellation logic", write "Verifies the cancellation logic passes successfully with the newly modified object initializers for patient, doctor, and staff arguments."
   - This does NOT apply when the PR adds brand-new test classes/methods for previously uncovered scenarios; in that case, simply describe what the new test verifies, since there is no prior behavior to tie it to.

3. **Layer separation**: only separate into "Domain" and "Application" sections when the PR has meaningful, testable changes in **both** layers. If changes are concentrated in a single layer, list the test classes directly without section headers.

4. **No method-level instructions**: reference test classes, not individual test methods. Do not say "test the `Cancel` method".

5. **Documentation-only PRs**: when the PR exclusively modifies XML documentation (`/// <summary>`, `/// <remarks>`, `/// <exception>`, etc.) with zero functional code changes, use this exact message instead of listing test classes:

```
No functional tests are required as these changes are strictly limited to XML documentation updates.
```

6. **AGENTS.md-only PRs**: when the PR exclusively modifies the `AGENTS.md` file, use this exact message:

```
No functional tests are required as these changes are strictly limited to project documentation updates in AGENTS.md.
```

Single-layer example:

```markdown
## How to test
- `ClinicalFormTemplateCommandValidatorBaseTests`: Verifies the shared validation rules, ensuring properties like `Name`, `Description`, and `JsonSchemaDefinition` conform to domain limits and formatting requirements.

- `CreateClinicalFormTemplateCommandHandlerTests` & `CreateClinicalFormTemplateCommandValidatorTests`: Verifies the end-to-end creation flow for new templates, ensuring repository interactions and validations are executed correctly.
```

Multi-layer example:

```markdown
## How to test
**Domain**:
- `AppointmentCancellationServiceTests`: Verifies the cancellation logic passes successfully with the newly modified object initializers for patient, doctor, and staff arguments.

---

**Application**:
- `CancelAppointmentByDoctorCommandHandler`: Ensures the cancellation flow respects the new `init`-only property requirements.
- `CancelAppointmentByPatientCommandHandler`: Verifies the argument mapping is correctly formed for patient-initiated cancellations.
- `CancelAppointmentByStaffCommandHandler`: Verifies the argument mapping is correctly formed for staff-initiated cancellations.
```

## Checklist

The checklist is filled by the **developer manually** before merging. The agent must leave all checkboxes unchecked.
