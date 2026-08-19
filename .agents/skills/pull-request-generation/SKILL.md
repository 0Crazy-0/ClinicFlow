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

3. **Formatting styles (Individual, Grouped, and Hybrid)**:
   - **Individual pattern (`- `TestClass`: Description`)**: use strictly for single test classes verifying distinct, specialized behaviors. Do not combine multiple test classes with `&` in a single bullet.
   - **Grouped pattern (Description header + bulleted test classes)**: use whenever 2 or more test classes share the same conceptual verification objective (e.g., query handlers mapping DTOs, paginated queries asserting metadata, repository persistence tracking, batch refactorings, or shared validation logic removals). State an overarching description starting with an active verb (e.g., "Verifies that...", "Confirms that...", "Ensures that...") ending with a colon, followed by the bulleted list of test classes.
   - **Hybrid pattern**: combine individual and grouped entries naturally within the same PR description or under layer sections when different tests require different levels of grouping.

4. **Layer separation**: only separate into "Domain", "Application", or "Infrastructure" sections when the PR has meaningful, testable changes in multiple layers (e.g., `**Domain**:`, `**Application**:`, `**Infrastructure**:`). If changes are concentrated in a single layer, list the test classes or groups directly without layer section headers.

5. **No method-level instructions**: reference test classes, not individual test methods. Do not say "test the `Cancel` method".

6. **Documentation-only PRs**: when the PR exclusively modifies XML documentation (`/// <summary>`, `/// <remarks>`, `/// <exception>`, etc.) with zero functional code changes, use this exact message instead of listing test classes:

```
No functional tests are required as these changes are strictly limited to XML documentation updates.
```

7. **AGENTS.md-only PRs**: when the PR exclusively modifies the `AGENTS.md` file, use this exact message:

```
No functional tests are required as these changes are strictly limited to project documentation updates in AGENTS.md.
```

### Individual pattern example (Single layer)

```markdown
## How to test
- `PatientTests`: Confirms that the simplified `CloseAccount` method correctly marks the primary user as deleted and enforces the rule that only primary users can close their accounts, reflecting the removal of the appointment state parameter.

- `PatientAccessServiceTests`: Validates that access verification now relies on pre-resolved boolean membership flags instead of direct entity relationship comparisons.

- `AppointmentCancellationServiceTests`: Verifies the emergency cancellation logic passes successfully using the newly introduced `IsInitiatorSelfOfTarget` and `IsInitiatorGuardianOfMinorTarget` flags instead of direct patient relationship properties.

- `ScheduleTests`: Verifies the `EnsureDoctorIsAvailable` validation logic on `Schedule` and the simplified schedule creation helpers.
```

### Grouped pattern example (Single layer)

```markdown
## How to test
Verifies that handlers returning a single data transfer object correctly map all properties, including nested value objects, without requiring redundant null checks:
- `GetAppointmentByIdQueryHandlerTests`
- `GetClinicalFormTemplateByCodeQueryHandlerTests`
- `GetDoctorByIdQueryHandlerTests`
- `GetPatientByIdQueryHandlerTests`

Verifies that handlers returning paginated results correctly assert structural equivalence for the inner items collection and explicitly validate all pagination metadata fields in both populated and empty scenarios:
- `GetAppointmentsByDateRangeQueryHandlerTests`
- `GetAppointmentsByDoctorIdAndDateQueryHandlerTests`
- `GetDoctorsBySpecialtyIdQueryHandlerTests`
- `GetMedicalRecordsByDoctorIdQueryHandlerTests`
```

### Multi-layer examples (Domain, Application & Infrastructure)

```markdown
## How to test
**Domain**:
- `FamilyMemberRegistrationServiceTests`: Verifies that registration throws `DomainValidationException` with the `FamilyMemberLimitExceeded` code when the active count reaches `MaxActiveFamilyMembers`, and confirms existing scenarios pass with the updated method signature.

---

**Application**:
- `AddFamilyMemberCommandHandlerTests`: Verifies the registration flow executes correctly through the new `ExecuteWithLockAsync` pass-through mock, ensuring the handler delegates to the lock wrapper without altering existing creation behavior.

---

**Infrastructure**:
- `PatientRepositoryTests`: Verifies that `CountActiveFamilyMembersAsync` correctly counts only active family members, excluding self patients, soft-deleted records, and family members belonging to other users.

- `UnitOfWorkLockTests`: Verifies advisory lock acquisition with single and dual keys, transaction commit and rollback behavior, domain event deferral until commit, suppression of events on failure, pending notification cleanup across successive operations, immediate publication when no active transaction exists, and concurrency blocking with identical versus distinct lock keys.
```

```markdown
## How to test
**Domain**:
- `FamilyMembershipTests`: Verifies the revocation domain rules, ensuring exceptions are thrown when attempting to revoke self-memberships, when the patient lacks an active self-membership, or when the minor has upcoming guardian-required appointments.

- `AgeEligibilityPolicyTests`: Verifies the new `RequiresGuardianForAge` method correctly evaluates guardian requirements based on the patient's age and policy configuration.

---

**Application**:
- `RevokeFamilyMemberCommandHandlerTests`: Verifies the end-to-end revocation flow, ensuring the handler correctly orchestrates repository checks, applies domain rules, and throws appropriate exceptions when business constraints are violated.

- `RevokeFamilyMemberCommandValidatorTests`: Verifies that the command validator enforces non-empty identifiers for both the owner user and the patient.

---

**Infrastructure**:
- `AppointmentRepositoryTests`: Verifies the new `HasUpcomingAppointmentRequiringGuardianForMinorAsync` query accurately identifies future appointments requiring a guardian while correctly excluding past, cancelled, or non-guardian appointments.

- `FamilyMembershipRepositoryTests`: Verifies repository interactions using the updated `Leave` method for self-memberships instead of `Revoke`.
```

### Hybrid & Multi-layer example

```markdown
## How to test
**Domain**:
Validates that the registration services no longer enforce the administrative claim rule for deleted patients, aligning with the updated domain model:
- `FamilyMemberRegistrationServiceTests`
- `PrimaryProfileRegistrationServiceTests`

---

**Application**:
- `DeactivateUserCommandHandlerTests`: Verifies the deactivation flow correctly terminates the self membership, enforces the active self membership requirement, and blocks deactivation when active family members exist.

- `ClosePatientAccountCommandHandlerTests`: Ensures the error message correctly references the user domain when active family members prevent account closure.

Confirms the removal of the deleted profile validation logic without affecting the remaining creation flows:
- `AddCompleteFamilyMemberCommandHandlerTests`
- `AddFamilyMemberCommandHandlerTests`
- `CreateCompletePatientProfileCommandHandlerTests`
- `CreatePatientProfileCommandHandlerTests`
```

## Checklist

The checklist is filled by the **developer manually** before merging. The agent must leave all checkboxes unchecked.
