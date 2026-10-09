---
applyTo: "CsharpTestAutomation.Tests/UI/**/*.cs,CsharpTestAutomation.Tests/Tests/UI/**/*.cs,CsharpTestAutomation.Tests/Tests/UiTestBase.cs,CsharpTestAutomation.Tests/Steps/UI/**/*.cs,CsharpTestAutomation.Bdd.Tests/UI/**/*.cs,CsharpTestAutomation.Bdd.Tests/Steps/UI/**/*.cs,CsharpTestAutomation.Bdd.Tests/Features/*Ui.feature"
trigger: glob
globs: "CsharpTestAutomation.Tests/UI/**/*.cs, CsharpTestAutomation.Tests/Tests/UI/**/*.cs, CsharpTestAutomation.Tests/Tests/UiTestBase.cs, CsharpTestAutomation.Tests/Steps/UI/**/*.cs, CsharpTestAutomation.Bdd.Tests/UI/**/*.cs, CsharpTestAutomation.Bdd.Tests/Steps/UI/**/*.cs, CsharpTestAutomation.Bdd.Tests/Features/*Ui.feature"
description: "Application page objects, components, UI fixtures, UI steps, locators, readiness, and Playwright assertions."
---

# Application UI Rules

Read the [shared test rules](test-automation.md). For UI layer design or new
page objects/components, read [UI architecture](../../docs/UI_TESTING_ARCHITECTURE.md).
Helpers follow these rules when their responsibility involves UI behavior.
If a UI scenario uses API setup or verification, also read [API rules](api-testing.md).

## Page objects, steps, and fixtures

- Page objects and components live in `UI/`; NUnit fixtures live in `Tests/UI/`.
  BDD UI step definitions live in `Steps/UI/` and follow [BDD rules](bdd-testing.md).
  In NUnit, repeated workflows above page objects belong in `Steps/UI/<App>/`.
- Page objects own locators, individual actions, and readiness; fixtures/Then bindings own assertions.
  Shared fragments use components and composition.
- In `CsharpTestAutomation.Tests`, use `UiTestBase`; its inherited `[Category("UI")]` provides pipeline filtering.
  Do not repeat inherited `[TestFixture]`, `[AllureNUnit]`, or the category.
- After navigation, call `WaitUntilLoadedAsync()` using `LongTimeoutInMS`, then
  perform actions and assertions. Authentication waits for semantic signed-in readiness.
- Follow the shared data/cleanup rules and applicable feature specification.

## Locators

Use this order; CSS/XPath is a strict last resort:

1. `GetByRole(AriaRole.<Role>, new() { Name = "..." })` for interactive elements/headings.
2. `GetByLabel("...")` for fields with associated labels.
3. `GetByPlaceholder("...")` for inputs without visible labels.
4. `GetByText("...")` for static text, notifications, alerts, or status chips.
5. `GetByTestId("...")` when semantic locators are absent, dynamic, or unstable.
6. CSS/XPath only after these options; CSS containers must lack accessible roles.

In `BaseUIComponent`, build child locators from `Root`, never `Page`, to isolate instances.
Validate new locators with the Playwright CLI when the app is available; read
the [tooling instructions](../instructions/tooling.md) before browser work.

## Assertions

- Use Playwright web-first `Expect(...)` for UI checks. Never add manual sleeps
  (`Thread.Sleep`, `Task.Delay`, `Page.WaitForTimeoutAsync`) or post-login delays.
- Playwright `Expect` throws `PlaywrightException`, which stops a NUnit
  `EnterMultipleScope`; that scope cannot collect multiple Playwright failures.
  If every independent check must run, capture each failure and report the
  failures after all checks. Do not swallow failures.
- Use the shared AwesomeAssertions/NUnit rules for value assertions and preconditions.
- Verify DB state only when it is part of the behavior under test.
