---
applyTo: "CsharpTestAutomation.Tests/**/*.cs"
trigger: glob
globs: "CsharpTestAutomation.Tests/**/*.cs"
description: "Shared application test naming, assertions, attributes, data ownership, cleanup, and steps."
---

# Application Test Rules

Applies to `CsharpTestAutomation.Tests`, which targets `net10.0` with nullable
enabled and references the framework. Read the shared [C# rules](csharp.md).
These rules apply by responsibility: fixture metadata is required on fixtures,
not on clients, DTOs, or page objects.

For API clients, DTOs, builders, fixtures, or steps, also read [API rules](api-testing.md).
For page objects, components, UI fixtures, or steps, also read [UI rules](ui-testing.md).
Supporting helpers follow the rules for the layers they use; cross-layer work reads both.

## Folder map

- `Database/<App>/Queries/`: Queries and connection-base behavior.
- `Database/<App>/DTO/`: DB-row records.
- `TestData/API/`: Shared static API data; `TestData/<App>/`: Constants and datasets.
- `Steps/API/<App>/`, `Steps/UI/<App>/`: Reusable workflows above clients/page objects.
- `Tests/API/`, `Tests/UI/`: Fixtures. Layer rules own client and page-object paths.

## Comments and async

- Keep `// Arrange`, `// Act`, and `// Assert` markers in test fixtures.
- Otherwise follow the shared C# comment rules. XML docs on application helpers
  should add constraints, setup, ownership, or override semantics.
- Use plain `await`; do not use `.ConfigureAwait(false)` in this project.

## Test cases and names

- Name test classes `<Subject>Tests`.
- Prefer `Verify_[ExpectedBehavior]_When_[StateUnderTest]` when state adds context,
  such as `Verify_ArgumentNullExceptionThrown_When_IdIsNull`. Use
  `Verify_[ExpectedBehavior]` when clear, such as `Verify_MaxPasswordLength_Is32`.
- Prefer data-driven cases over near-duplicate test methods. Each edge/negative
  input must have its own reported result.
- For static inputs, use `[TestCase(..., TestName = "...")]` with stable, readable
  names in runner/TRX output; avoid long or random parameter values in names.
- Use `[TestCaseSource]` for generated or larger datasets when naming is stable.
  Use separate `[Test]` methods when dynamic values make that impractical.
  Attribute arguments must be compile-time constants.

## Attributes

- Fixtures: `[AllureSuite]` and `[AllureFeature]`.
- Tests: `[AllureStory]`, `[AllureSeverity]`, and `[AllureOwner]`.
- Approved defect regressions only: `[Category("KnownDefect")]` and `[AllureIssue("<id>")]`.
- `[AllureDescription]` is optional when the scenario needs explanation.
- Layer rules define inherited fixture/category attributes; do not duplicate them.

## Assertions

- Prefer AwesomeAssertions for values. Compare whole objects with
  `actual.Should().BeEquivalentTo(expected, o => o.ExcludingMissingMembers())`.
- Single-field assertions are valid for partial contracts, volatile/external
  values, or a field's validation rule; state the reason in the test name or message.
- Group independent AwesomeAssertions in `using (new AssertionScope())`.
  Group NUnit assertions in `using (Assert.EnterMultipleScope())`.
- Keep critical preconditions outside multiple-assertion scopes so they fail
  before response extraction or dependent operations. For example,
  `response.StatusCode.Should().Be(HttpStatusCode.Created);` before extracting
  `response.Data` or querying the DB.
- Assert required targets are non-null before checking members. Null-conditional
  assertions skip null targets; use them only when null is accepted and checked
  separately, or the assertion is intentionally optional.
- Playwright failures stop a NUnit multiple-assertion scope. Follow the
  [UI assertion rules](ui-testing.md#assertions) when each independent check must run.

## DB verification

Verify persisted state on API write success paths when persistence is part of
the contract. Require DB checks for reads and UI tests only when DB state is part
of the behavior under test. When comparing with a DB record, verify the complete
returned contract. Name exclusions for absent, generated/volatile, or externally
sourced fields. Check external fields for their behavior or expected value.

Alias SQL columns to DB-row C# member names; tests must not translate
`snake_case` DB names to DTO properties manually.

## Data ownership and cleanup

- Query existing data first through `Database/<App>/Queries/`; reuse row DTOs.
  Records the test did not create are read-only: never mutate or delete them.
- For a scenario that owns setup, create required records through the API or a
  DB helper. Register cleanup immediately, before validating the response or
  creating dependents. Use the fixture's inherited `ScenarioCleanupActions`.
- Never open a connection or embed a connection string in a test. Query methods
  acquire short-lived connections from `PostgreSqlConnectionPool` through
  `<App>Database.Run(...)`, which disposes the connection.
- Prefer Bogus `Faker`/`Randomizer` and the existing generator pattern for unique
  or varied test values. Fixed domain values remain constants.
- Register prerequisite cleanup in creation order so LIFO deletion removes
  dependents first. For UI creation, register lookup-based cleanup before
  submission and identify only the test-owned record if later operations fail.
  Follow the relevant feature specification for identifiers and delete outcomes.
- Missing environment seed data and missing records created by the test are
  different failures; [API rules](api-testing.md#prerequisites) define handling.

## Reusable steps

Put executable workflows and prerequisites in `Steps/API/<App>/` or
`Steps/UI/<App>/`, with feature/workflow names ending in `Steps`.
Use `Steps.Api.<App>` or `Steps.UI.<App>` in namespaces. API steps may serve UI
fixtures. Keep small result records beside their steps; constants/generated
values stay in `TestData/`, and request builders stay in `API/Factories/`.

Steps compose clients or page objects when a repeated workflow needs coordination;
avoid one-to-one wrappers. Clients own endpoint requests. Page objects own
locators, individual UI actions, and readiness. Neither depends on steps or fixtures.

Steps borrow fixture-registered clients and its cleanup stack; they do not own
clients or create another cleanup stack. Keep mutable step state per test.
Prerequisite steps may reject failed setup responses; operations under test
return native `RestResponse<T>` to the fixture. Tests own behavior assertions.

## Completion

- Build warning-clean and run the relevant formatter/analyzer checks.
- Changed live API/UI fixtures pass three consecutive headless runs when services
  and credentials are available. For clients, helpers, DTOs, or other changes,
  use relevant local checks and focused tests; report unavailable live validation.
- Created records have cleanup registered, contract-relevant DB checks are present,
  and fixture/test Allure metadata is complete.
- Approved known defects have a report at `docs/known-defects/<id>.md` and a test
  tagged `[Category("KnownDefect")]` plus `[AllureIssue(...)]`.
- Update the owning instruction when introducing a new convention or pattern.
