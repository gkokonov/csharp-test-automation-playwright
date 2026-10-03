---
applyTo: "CsharpTestAutomation.Tests/**/*.cs"
trigger: glob
globs: "CsharpTestAutomation.Tests/**/*.cs"
description: "Application test, client, DTO, builder, page object, and test data conventions."
---

# CsharpTestAutomation.Tests — Authoring Instructions

Conventions for the **application test project** (`CsharpTestAutomation.Tests`):
typed API clients, DTOs, DTO builders, page objects, test data, and the tests
that exercise the application under test. Targets `net10.0` with
`<Nullable>enable</Nullable>` and references `CsharpTestAutomation.Framework`.

## Folder Map

- `API/Clients/`: Typed API clients in `CsharpTestAutomation.Tests.Api.Clients`.
- `API/DTOs/<Resource>/`: Request/response DTOs, one sub-folder per resource;
  namespace `CsharpTestAutomation.Tests.Api.Dtos.<Resource>`.
- `API/Factories/`: NBuilder-backed DTO builders, such as
  `CreateResourceDtoBuilder : BaseBuilder<CreateResourceDto>`, in
  `CsharpTestAutomation.Tests.Api.Factories`.
- `Database/<App>/Queries/`: Resource queries and connection-base behaviour.
- `Database/<App>/DTO/`: DB-row records.
- `TestData/API/`: Static or well-known test data shared by API fixtures.
- `Tests/API/`, `Tests/UI/`: Test fixtures.
- `UI/`: Page objects and components.

**Casing rule:** folders are upper-case `API/DTOs/`; namespaces are Pascal-case
`Api.Dtos`. This is intentional and analyzer-clean because
`dotnet_style_namespace_match_folder` compares case-insensitively. Use the
upper-case form in file paths and the Pascal-case form in `namespace` and
`using` declarations. Do not "correct" either one.

## Comments

Prefer self-explanatory code over comments. Make methods, classes, and variables
reveal intent through their names so a reader rarely needs prose.

- **Production / non-test code** (typed API clients, page objects, DTOs, builders,
  helpers): avoid comments that merely restate what the code already says. If a
  better method or variable name removes the need for a comment, prefer renaming
  the symbol.
- **Test fixtures**: **keep** the `// Arrange` / `// Act` / `// Assert` section
  markers. They are an intentional, project-wide convention that signals each
  phase of a test and is **not** treated as a redundant comment.
- **Only** add other comments when behaviour is genuinely non-obvious. Explain
  the *why*, a tradeoff, a workaround, an external-system quirk, or a deliberately
  disabled code path. Examples worth keeping: rate-limit/parallelism rationale,
  "Umbraco CMS can be slow sometimes", or why a modal-handling block is
  commented out.
- Keep XML doc comments only where they add information a self-explanatory
  signature cannot (non-obvious side effects, per-request override/suppression
  semantics, required setup).

## Async

Do **not** use `.ConfigureAwait(false)` in this project — use plain `await`.
NUnit relies on its single-threaded synchronization context, so suppressing
context capture is unwanted here. `.ConfigureAwait(false)` belongs only in the
reusable `CsharpTestAutomation.Framework` library.

## API Test Fixtures

Every API test fixture derives from `ApiTestBase` — that is the reason the
class exists, so a plain `[TestFixture]` is never used for an API test, even
when the fixture has no need for `RequireDbData`. Do not duplicate
`[AllureNUnit]`, `[TestFixture]`, or `[Category("API")]` on the fixture; they
are already applied by `TestBase`/`ApiTestBase`.

- Build each typed client in an overridden `OnSetUpAsync()`
  (`await base.OnSetUpAsync()` first, which populates the inherited
  `RestClientFactory`), using the inherited `RestClientFactory` and
  `NetBoxAuthenticator`. Register it with `ApiTestBase.RegisterClient(...)`
  (**not** a plain field), and expose it to test methods through a computed
  property backed by `ApiTestBase.GetClient<T>()`:

  ```csharp
  private FooApiClient FooClient => GetClient<FooApiClient>();

  protected override async Task OnSetUpAsync()
  {
      await base.OnSetUpAsync();
      RegisterClient(new FooApiClient(RestClientFactory, NetBoxAuthenticator));
  }
  ```

  `RegisterClient`/`GetClient` are thin, intention-revealing wrappers around
  `TestContainer.Register`/`Get` — fixtures never call `TestContainer` directly.
  The reason a plain disposable field does not work: `TestBase` disposes
  everything registered via `RegisterClient` only **after**
  `ScenarioCleanupActions.CleanUpAsync()` runs, so the client stays usable for
  any delete call a cleanup action performs; a plain field would also trip the
  `NUnit1032` analyzer, since nothing in the fixture's own (non-existent)
  `[TearDown]` disposes it, and suppressing that analyzer is not an option (see
  `.agents/rules/csharp.md` on not suppressing diagnostics to pass validation).
- Register cleanup via the inherited `ScenarioCleanupActions` property, never a
  locally-constructed `ScenarioCleanupActions` instance.

## Test Case Authoring

Prefer data-driven tests over creating many near-duplicate standalone test
methods.

- For static inputs, use `[TestCase(..., TestName = "...")]` so each iteration is
  a separate, readable test case in `dotnet test` output and Azure DevOps
  (TRX-based) results.
- Keep test names explicit and behaviour-oriented using
  `Verify_[ExpectedBehavior]_When_[StateUnderTest]`
  (e.g. `Verify_ArgumentNullExceptionThrown_When_IdIsNull`,
  `Verify_DiscountApplied_When_UserIsPremium`). Avoid raw long/random
  parameter values as visible test names.
- For runtime-generated inputs (for example `Guid.NewGuid()`), `[TestCase]`
  cannot be used because attribute arguments must be compile-time constants. In
  these cases, use a small number of separate `[Test]` methods with stable names,
  or use `[TestCaseSource]` when the runner/adapters preserve the intended case
  names.
- If a single method contains multiple negative/edge assertions for different
  inputs, refactor it so each input is reported as its own test case result.

Default preference order for this repository:

1. Data-driven with `[TestCase(..., TestName = "...")]` for static datasets.
2. `[TestCaseSource]` when generated or larger datasets are required and naming
   is stable.
3. Separate `[Test]` methods only when dynamic values make attribute-based
   parameterization impractical.

## Required Attributes

- Fixtures: `[AllureSuite]` and `[AllureFeature]` for report grouping.
- Tests: `[AllureStory]`, `[AllureSeverity]`, and `[AllureOwner]` for report
  metadata.
- Approved defect regressions only: `[Category("KnownDefect")]` and
  `[AllureIssue("<id>")]` on the test.
- UI fixtures: `[Category("UI")]`, applied by `UiTestBase` and used by pipeline
  filters.

`[AllureDescription]` is optional; add it when the scenario is not obvious from
the test name.

## Assertions and Multiple Assertions

Assert whole objects, not field by field:
`actual.Should().BeEquivalentTo(expected, o => o.ExcludingMissingMembers())`.
Assert a single field only when the contract is intentionally partial, the value
is volatile or externally sourced, or the test targets that field's own
validation rule — state the reason in the test name or assertion message.

- **Single vs Multi-Assertion**:
  - **Default (Whole Object)**: Use `Should().BeEquivalentTo(...)`. It naturally
    evaluates all properties and reports all mismatches together.
  - **Multiple Independent Assertions**: For separate conditions (for example,
    status code, headers, or payload fields), use
    `Assert.Multiple(() => { ... })` or AwesomeAssertions
    `using (new AssertionScope()) { ... }`. Both report all failures instead of
    stopping at the first one.
  - **Asynchronous Assertions**: For multiple Playwright or asynchronous checks,
    use NUnit 4's `await Assert.MultipleAsync(async () => { ... })`.
  - **Critical Preconditions**: Keep gate checks outside `Assert.Multiple` so
    they fail fast before downstream operations. For example, assert
    `response.StatusCode == HttpStatusCode.Created` before extracting
    `response.Data` or querying the DB.

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

UI tests use Playwright web-first `Expect(...)`; never manual sleeps
(`Thread.Sleep`, `Task.Delay`, or `Page.WaitForTimeoutAsync`). Authentication
setup waits for a semantic signed-in readiness signal. Do not add a fixed
post-login delay.

- **Locator Priority Hierarchy**:
    1. `GetByRole(AriaRole.<Role>, new() { Name = "..." })` — primary choice for
      interactive elements (buttons, links, headings, checkboxes). Mirrors
      accessible user behavior.
    2. `GetByLabel("...")` — primary choice for form fields and inputs with associated labels.
    3. `GetByPlaceholder("...")` — use for inputs that lack visible text labels.
    4. `GetByText("...")` — use for static text, notifications, alerts, and
      non-interactive status chips.
    5. `GetByTestId("...")` — use when semantic locators are absent, dynamic, or unstable.
    6. **CSS / XPath** — strict last resort after `GetByRole`, `GetByLabel`,
      `GetByPlaceholder`, `GetByText`, and `GetByTestId`. Use CSS only for
      third-party layout containers that lack accessible roles.
- **Component Scoping**: In components deriving from `BaseUIComponent`, scope
  child locators from `Root` (for example, `Root.GetByRole(...)`), never `Page`,
  to prevent locator bleeding across instances.
- **Readiness**: After navigation, enforce page stability with
  `WaitUntilLoadedAsync()` (uses `LongTimeoutInMS`), then execute actions and
  assertions.

## Test Data

1. Query existing data first — reuse `Database/<App>/Queries`, or add a method
  there following the folder's pattern. Reuse its row DTOs for whole-object
  `BeEquivalentTo` verification.
2. Found records are **read-only**. Never mutate, update, or delete data the test
   did not create.
3. For API tests whose scenario requires seeded environment data, use
  `ApiTestBase.RequireDbData<T>` for nullable or empty results. Missing required
  seed data makes the test inconclusive; do not create unrelated fallback data.
  Do not use `RequireDbData` for a row the test just created. A missing created
  row is a failed assertion, not missing seed data.
4. When the scenario owns its setup (for example, an API create/write test),
  create the required record through the API or a DB helper and register
  cleanup with `ScenarioCleanupActions` immediately after creation. Never
  mutate, update, or delete a record the test did not create.
5. Never open a connection or embed a connection string in a test. Call the
`Database/<App>/Queries` repository methods; they take no connection. Each
repository acquires a short-lived connection from the shared
`PostgreSqlConnectionPool` through its `<App>Database.Run(...)` helper, which
disposes the connection after the query.
6. Build request payloads with the DTO builders in `API/Factories/`
  (`CreateResourceDtoBuilder : BaseBuilder<CreateResourceDto>`) rather than
  constructing DTOs inline. Keep shared static values in `TestData/API/`.
7. Prefer **Bogus** (`Faker`/`Randomizer`) over raw `Guid.NewGuid()` or
  `Random`/`Random.Shared` calls for generated/unique test-data values (names,
  slugs, free-form identifiers, synthetic addresses, and similar). A single
  shared `static readonly Faker` instance is safe across concurrently executing
  fixtures — Bogus documents its `Randomizer` primitives (the `Faker.Random`
  facet) as thread-safe. Fixed, domain-constrained values (for example an
  enum's literal members, or a short human-readable label meant to stay
  recognizable in logs/UI) remain plain constants; only values that exist to be
  unique or varied move to Bogus.

## Definition of Done

1. `dotnet build` is warning-clean (`TreatWarningsAsErrors`).
2. The fixture passes three consecutive headless runs.
3. API write success paths verify persisted state when database persistence is
  part of the contract. Read and UI tests verify DB state only when it is part
  of the behavior under test.
4. Every created record registers cleanup at creation time.
5. Allure metadata is complete: suite, feature, story, severity, owner.
6. Any approved known defect has a report at `docs/known-defects/<id>.md` and a
  regression test tagged `[Category("KnownDefect")]` + `[AllureIssue(...)]`.
7. No new pattern was introduced without updating the owning instruction file in
   the same change.

## Layer References

API clients and API tests → `docs/API_TESTING_ARCHITECTURE.md`
Page objects and UI tests → `docs/UI_TESTING_ARCHITECTURE.md`
