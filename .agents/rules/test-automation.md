---
applyTo: "CsharpTestAutomation.Tests/**/*.cs"
trigger: always_on
description: "Authoring conventions for application tests, typed clients, DTOs, builders, page objects and test data."
---

# CsharpTestAutomation.Tests — Authoring Instructions

Conventions for the **application test project** (`CsharpTestAutomation.Tests`):
typed API clients, DTOs, DTO builders, page objects, test data, and the tests
that exercise the application under test. Targets `net10.0` with
`<Nullable>enable</Nullable>` and references `CsharpTestAutomation.Framework`.

## Folder Map

| Path | Contents | Namespace |
| --- | --- | --- |
| `API/Clients/` | Typed API clients | `CsharpTestAutomation.Tests.Api.Clients` |
| `API/DTOs/<Resource>/` | Request/response DTOs, one sub-folder per resource | `CsharpTestAutomation.Tests.Api.Dtos.<Resource>` |
| `API/Factories/` | DTO builders (`CreateCpfDtoBuilder : BaseBuilder<CreateCpfDto>`), NBuilder-backed | `CsharpTestAutomation.Tests.Api.Factories` |
| `Database/CPF/Queries/` | Resource queries and connection-base behaviour | — |
| `Database/CPF/DTO/` | DB-row records | — |
| `TestData/API/` | Static/well-known test data (`CpfTestData`, `WellKnownUsers`) | — |
| `Tests/API/`, `Tests/UI/` | Test fixtures | — |
| `UI/` | Page objects and components | — |

**Casing rule:** folders are upper-case `API/DTOs/`; namespaces are Pascal-case
`Api.Dtos`. This is intentional and analyzer-clean because
`dotnet_style_namespace_match_folder` compares case-insensitively. Use the
upper-case form in file paths and the Pascal-case form in `namespace` and
`using` declarations. Do not "correct" either one.

## Comments

Prefer self-explanatory code over comments. Make methods, classes, and variables
reveal intent through their names so a reader rarely needs prose.

* **Production / non-test code** (typed API clients, page objects, DTOs,
  builders, helpers): avoid comments that merely restate what the code already
  says. If a better method or variable name removes the need for a comment, prefer
  renaming the symbol.
* **Test fixtures**: **keep** the `// Arrange` / `// Act` / `// Assert` section
  markers — they are an intentional, project-wide convention that signals each
  phase of a test and is **not** treated as a redundant comment.
* **Only** add other comments when behaviour is genuinely non-obvious — explain
  the *why*, a tradeoff, a workaround, an external-system quirk, or a
  deliberately disabled code path. Examples worth keeping: rate-limit/parallelism
  rationale, "Umbraco CMS can be slow sometimes", or why a modal-handling block
  is commented out.
* Keep XML doc comments only where they add information a self-explanatory
  signature cannot (non-obvious side effects, per-request override/suppression
  semantics, required setup).

## Async

Do **not** use `.ConfigureAwait(false)` in this project — use plain `await`.
NUnit relies on its single-threaded synchronization context, so suppressing
context capture is unwanted here. `.ConfigureAwait(false)` belongs only in the
reusable `CsharpTestAutomation.Framework` library.

## Test Case Authoring

Prefer data-driven tests over creating many near-duplicate standalone test
methods.

* For static inputs, use `[TestCase(..., TestName = "...")]` so each iteration is
  a separate, readable test case in `dotnet test` output and Azure DevOps
  (TRX-based) results.
* Keep test names explicit and behaviour-oriented
  (`<Scenario>_<Condition>_<ExpectedResult>`), and avoid using raw long/random
  parameter values as visible test names.
* For runtime-generated inputs (for example `Guid.NewGuid()`), `[TestCase]`
  cannot be used because attribute arguments must be compile-time constants. In
  these cases, use a small number of separate `[Test]` methods with stable names,
  or use `[TestCaseSource]` when the runner/adapters preserve the intended case
  names.
* If a single method contains multiple negative/edge assertions for different
  inputs, refactor it so each input is reported as its own test case result.

Default preference order for this repository:

1. Data-driven with `[TestCase(..., TestName = "...")]` for static datasets.
2. `[TestCaseSource]` when generated or larger datasets are required and naming
   is stable.
3. Separate `[Test]` methods only when dynamic values make attribute-based
   parameterization impractical.

## Required Attributes

| Attribute | Scope | Purpose |
| --- | --- | --- |
| `[AllureSuite]`, `[AllureFeature]` | Fixture | Report grouping |
| `[AllureStory]`, `[AllureSeverity]`, `[AllureOwner]` | Test | Report metadata |
| `[Category("KnownDefect")]` + `[AllureIssue("<id>")]` | Test | Approved defect regressions only |
| `[Category("UI")]` | Fixture | Applied by `UiTestBase`; used by pipeline filters |

`[AllureDescription]` is optional; add it when the scenario is not obvious from
the test name.

## Assertions and Multiple Assertions

Assert whole objects, not field by field:
`actual.Should().BeEquivalentTo(expected, o => o.ExcludingMissingMembers())`.
Assert a single field only when the contract is intentionally partial, the value
is volatile or externally sourced, or the test targets that field's own
validation rule — state the reason in the test name or assertion message.

* **Single vs Multi-Assertion**:
  * **Default (Whole Object)**: Use `Should().BeEquivalentTo(...)` — it naturally evaluates all properties and reports all mismatches together.
  * **Multiple Independent Assertions**: When validating multiple separate conditions (e.g. status code, headers, individual payload fields), wrap them in `Assert.Multiple(() => { ... })` or AwesomeAssertions `using (new AssertionScope()) { ... }`. This prevents early test termination on the first failing assertion and reports all failures at once.
  * **Asynchronous Assertions**: For multiple Playwright or asynchronous checks, use NUnit 4's `await Assert.MultipleAsync(async () => { ... })`.
  * **Critical Preconditions**: Gate checks (e.g. asserting `response.StatusCode == HttpStatusCode.Created` before extracting `response.Data` or querying the DB) must remain outside `Assert.Multiple` to fail fast before downstream operations.

Every success-path scenario verifies the complete returned object against the
actual DB record. Exclude only members the endpoint does not return, that are
generated/volatile, or that come from an external service — and name the
exclusion in the assertion. Externally sourced fields are verified for behaviour
(non-null, or a known expected value), not against the DB.

PostgreSQL tables and columns commonly use `snake_case`, while API DTO
properties use `camelCase` JSON and PascalCase C# members. Alias SQL columns to
the DB-row record's C# member names; never make a test translate property names
manually.

Use AwesomeAssertions for status codes, headers, content type, and deserialized
payloads. Use the minimal `ApiAssertions` extensions only for transport status
(`ShouldHaveCompletedTransport`) and JSONPath (`ShouldHaveJsonPathValue`).

## UI Locators and Assertions

UI tests use Playwright web-first `Expect(...)`; never manual sleeps (`Thread.Sleep`, `Task.Delay`, or `Page.WaitForTimeoutAsync`).

* **Locator Priority Hierarchy**:
  1. `GetByRole(AriaRole.<Role>, new() { Name = "..." })` — primary choice for interactive elements (buttons, links, headings, checkboxes). Mirrors accessible user behavior.
  2. `GetByLabel("...")` — primary choice for form fields and inputs with associated labels.
  3. `GetByPlaceholder("...")` — use for inputs that lack visible text labels.
  4. `GetByText("...")` — use for static text, notifications, alerts, and non-interactive status chips.
  5. `GetByTestId("...")` — use when semantic locators are absent, dynamic, or unstable.
  6. **CSS / XPath** — strict last resort: `GetByRole` → `GetByLabel` → `GetByPlaceholder` → `GetByText` → `GetByTestId` → CSS. Use CSS only for third-party layout containers lacking accessible roles.
* **Component Scoping**: In components deriving from `BaseUIComponent`, always scope child locators from `Root` (e.g. `Root.GetByRole(...)`), never `Page`, to prevent locator bleeding across instances.
* **Readiness**: Enforce page stability with `WaitUntilLoadedAsync()` (uses `LongTimeoutInMS`) after navigation, then execute actions and assertions.

## Test Data

1. Query existing data first — reuse `Database/CPF/Queries`, or add a method
   there following the folder's pattern. Reuse the row DTO in `Database/CPF/DTO`
   for whole-object `BeEquivalentTo` verification.
2. Found records are **read-only**. Never mutate, update, or delete data the test
   did not create.
3. If nothing qualifies, create it via the API or a DB helper and register
   cleanup with `ScenarioCleanupActions` immediately after creation.
4. Implement the create-own-data fallback even when environment data usually
   exists — a test must not depend solely on the environment.
5. Use `ApiTestBase.RequireDbData<T>` for nullable or empty results — never a
   manual null check followed by `Assert.Inconclusive`.
6. Never open a connection or embed a connection string in a test. Use the
   existing query base and the shared `PostgreSqlConnectionPool`.
7. Build request payloads with the DTO builders in `API/Factories/`
   (`CreateCpfDtoBuilder : BaseBuilder<CreateCpfDto>`) rather than constructing
   DTOs inline. Static and well-known values live in `TestData/API/`
   (`CpfTestData`, `WellKnownUsers`).

## Definition of Done

1. `dotnet build` is warning-clean (`TreatWarningsAsErrors`).
2. The fixture passes three consecutive headless runs.
3. Every success path verifies persistence against the DB row.
4. Every created record registers cleanup at creation time.
5. Allure metadata is complete: suite, feature, story, severity, owner.
6. Any known defect has both a Markdown defect report and a regression test
   tagged `[Category("KnownDefect")]` + `[AllureIssue(...)]`.
7. No new pattern was introduced without updating the owning instruction file in
   the same change.

## Layer References

API clients and API tests → `docs/API_TESTING_ARCHITECTURE.md`
Page objects and UI tests → `docs/UI_TESTING_ARCHITECTURE.md`
