---
applyTo: "CsharpTestAutomation.Bdd.Tests/**/*.cs,CsharpTestAutomation.Bdd.Tests/**/*.feature,CsharpTestAutomation.Bdd.Tests/reqnroll.json"
trigger: glob
globs: "CsharpTestAutomation.Bdd.Tests/**/*.cs, CsharpTestAutomation.Bdd.Tests/**/*.feature, CsharpTestAutomation.Bdd.Tests/reqnroll.json"
description: "Reqnroll features, bindings, hooks, typed context, metadata, lifecycle, and generated sources."
---

# BDD Application Rules

Read the [BDD scope](../../CsharpTestAutomation.Bdd.Tests/AGENTS.md), shared
[test rules](test-automation.md), and applicable API/UI rules. Shared assertion,
data ownership, page readiness, client, and cleanup policies apply. NUnit fixture
base classes and Allure.NUnit attributes apply to the NUnit application project.

## Features, steps, and state

- Features describe domain behavior without HTTP, SQL, or implementation details.
  Keep each operation under test in `When` and behavior assertions in `Then`.
- Constructor-inject scenario dependencies. Use typed state in `Context/`;
  do not use string-key `ScenarioContext`/`FeatureContext` dictionaries.
- Reqnroll step definitions live in `Steps/API/<App>/` or `Steps/UI/<App>/`
  with class names ending in `Steps`. Keep setup, actions, assertions, and cleanup
  registration in these step classes; do not add a separate workflow Steps layer.
  Clients own requests and pages own locators/actions/readiness. Step classes
  borrow the hook-registered client, typed state, and single cleanup stack.
  Small shared cleanup operations can remain in a step file without invoking
  another class's Given/When/Then methods. Do not move assertions into clients
  or page objects.
- Keep mutable state per scenario. Never cache a client, browser, page, or
  mutable scenario state in static fields or run-wide services.
- Product Site scenarios use `@NetBox @Site`, one of `@API`/`@UI`, and exactly
  one stable ID: `@SiteApi01` through `@SiteApi05` or `@SiteUi01` through `@SiteUi03`.
  Retain existing response, UI, persistence, and cleanup assertions.
- `SiteScenarioMetadata` owns story, severity, and owner by stable ID. Hooks add
  suite API/UI, epic NetBox, and one canonical Site Management feature. Use
  Allure.Reqnroll only; do not activate Allure.NUnit or add retry plugins.

## Hooks and resource ownership

- `CommonHooks` registers the single cleanup stack, typed state, and lifecycle
  before any dependent binding resolves. Site hooks set metadata before setup,
  create API dependencies, then create UI dependencies only for UI scenarios.
- Register each release immediately after acquisition. Scenario-owned resources
  injected into BoDi use `dispose:false`; `ScenarioLifecycle` is their one
  explicit release owner. Run services remain owned by `BddRunResources`.
- Teardown attempts each failure-evidence capture independently, then executes
  LIFO data cleanup while clients are alive, then releases resources in reverse
  acquisition order. Partial setup must still release acquired resources.
- Preserve an existing scenario error. Attach/report secondary errors without
  stopping later cleanup. Cleanup or explicit release failure after a passing
  body must fail both NUnit and Allure before the adapter emits the result.
  Do not assign `ScenarioContext.TestError` or silently swallow teardown errors.
- `BddRunResources` owns configuration, controlled token/pool initialization,
  and the lazy UI snapshot. A failed initialization is not cached; release the
  lock and partial resources before another attempt. API-only runs do not log in
  through the UI. Bootstrap inside a scenario's test context, close its browser,
  then use a fresh browser/context/page for each UI scenario.
- Each run owns a unique snapshot file; never log or attach it. Delete only
  that file at run end. Keep normal API request redaction enabled. Call
  process-wide `DisposeAllAsync` only after all scenarios complete.
- Feature fixtures run with two workers; scenarios within a feature are
  sequential. Do not introduce scenario-level parallelism without validating
  resource isolation and the browser factory's test-context contract.

## Generated files and validation

- Edit `.feature` sources, never generated `.feature.cs` files. Reqnroll clean/
  rebuild removes obsolete generated sources; keep them ignored by Git.
- Use the [BDD scope commands](../../CsharpTestAutomation.Bdd.Tests/AGENTS.md)
  and report required validation results.
  Use project selection plus `TestCategory=Site`, `API`, `UI`, or a stable ID.
