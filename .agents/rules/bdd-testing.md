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

Application names, scenario inventories, report labels, and environment settings
belong in application specifications, feature tags, and configuration. The project
examples below illustrate the rules; they do not define requirements for other
applications.

## Feature specifications

- Describe domain behavior without HTTP, SQL, locators, or implementation details.
  Put prerequisites in `Given`, the operation under test in `When`, and observable
  outcomes in `Then`. Setup checks can fail early when a prerequisite is invalid.
- Keep scenarios independent. Do not depend on data or execution order from
  another scenario. Use `Background` only for prerequisites shared by every
  scenario; use `Scenario Outline` when input variations test the same behavior.
- State meaningful values and transitions in Gherkin, such as an active Site
  changing to `planned`. Builder defaults supply incidental data only.
- Preserve distinct behaviors and contract-relevant assertions when converting
  existing tests. Keep application coverage inventories in their specifications.

Example from [SiteApi.feature](../../CsharpTestAutomation.Bdd.Tests/Features/SiteApi.feature):

```gherkin
Scenario: Creating a Site returns its requested details
    Given unique valid Site details
    When the Site is created through the service
    Then the requested Site details are returned
    And the Site details are saved
```

## Step definitions and scenario state

- Constructor-inject scenario dependencies. Use typed state in `Context/`;
  do not use string-key `ScenarioContext`/`FeatureContext` dictionaries.
- Group state by feature responsibility, not by endpoint. Store the requests,
  prerequisites, expected records, responses, and page objects that the scenario
  needs. Reuse response-state composition when the response contracts match;
  do not require every feature to have single, search, and deletion responses.
- Keep mutable state per scenario. Never cache a scenario client, browser, page,
  or mutable scenario state in static fields or run-wide services.
- Put definitions in `Steps/API/<App>/` or `Steps/UI/<App>/`, with class names
  ending in `Steps`. Keep setup, actions, assertions, and cleanup registration
  there; do not add a separate workflow Steps layer.
- Clients own requests; pages own locators, actions, and readiness. Definitions
  borrow hook-registered clients, typed state, and the single cleanup stack.
  Keep behavior assertions in `Then` bindings, not clients or page objects.
- Assertion bindings must not prepare state for another assertion. Persistence
  checks consume the action response, or independently identify a UI-created
  record from its unique request. Check preconditions before reading details.
- Scope bindings when applications or features use overlapping step text. Share
  small operations as ordinary methods, not calls to another class's bindings.

Example from [SiteScenarioState](../../CsharpTestAutomation.Bdd.Tests/Context/SiteScenarioState.cs):
each scenario owns its requests and a fresh response-state instance.

```csharp
public CreateSiteDto CreateRequest { get; set; } = null!;
public UpdateSiteDto UpdateRequest { get; set; } = null!;
public ApiResponseState<SiteDetailDto> ApiResponses { get; } = new();
```

## Tags and reporting

- Use tags for the application, feature area, execution layer, and one stable
  scenario ID. API/UI scenarios have exactly one of `@API` or `@UI`. Define names
  and ID inventories in the application's feature sources.
- Put report metadata directly in feature/scenario Allure tags: `@critical`,
  `@allure.label.owner:AutomationTeam`, `@allure.label.suite:API`, and
  `@allure.label.epic:NetBox`. Feature tags inherit to scenarios. Use the native
  feature title and scenario title; do not duplicate them in metadata classes,
  hooks, or story tags. Tags cannot contain spaces.
- The BDD Allure configuration accepts `@link:https://dev.example.com/`,
  `@issue:AUTH-123`, and `@tms:TMS-456`, plus their `@allure.` forms. Add real
  references only when the scenario has them.
- Use Allure.Reqnroll only in the BDD project; do not activate Allure.NUnit or
  add retry plugins.

Project example: `@NetBox @Site @API` and `@SiteApi01` select an application,
feature, layer, and scenario. Report tags live beside those stable IDs in the
feature sources.

## Hooks and resource ownership

- Common hooks register one cleanup stack and the scenario lifecycle before
  dependent bindings resolve. Scoped feature hooks register their state and
  API dependencies. Create browser resources only for UI scenarios.
- Register each release immediately after acquisition. Give each resource one
  release owner. Resources managed by the explicit scenario lifecycle use
  `dispose: false` when registered in BoDi. Run services have a run-level owner.
- Teardown attempts failure evidence independently, executes LIFO data cleanup
  while clients are alive, then releases resources in reverse acquisition order.
  Register prerequisite cleanup before dependent cleanup. Partial setup must
  still release all acquired resources.
- Preserve an existing scenario error and report secondary errors without
  stopping cleanup. Cleanup or release failure after a passing body must fail
  both NUnit and Allure before the adapter emits the result. Do not assign
  `ScenarioContext.TestError` or silently swallow teardown errors.
- Use framework `AllureExtensions` for screenshots, browser logs, and retained
  traces. Finalize traces before context release, including partial setup.

Project examples: [CommonHooks](../../CsharpTestAutomation.Bdd.Tests/Hooks/CommonHooks.cs)
registers common cleanup; [SiteHooks](../../CsharpTestAutomation.Bdd.Tests/Hooks/SiteHooks.cs)
registers Site state and dependencies; [ScenarioLifecycle](../../CsharpTestAutomation.Bdd.Tests/Lifecycle/ScenarioLifecycle.cs)
controls evidence, data cleanup, and resource release order.

## Run resources and isolation

- Check application HTTP dependencies with `DependencyAvailabilityChecker`
  after metadata and before setup. Cache successful checks per run under a
  lock; check the UI only for UI scenarios and allow another attempt after failure.
- Keep configuration and shared authentication or pool initialization under a
  run-level owner. Do not cache failed initialization; release locks and partial
  resources before another attempt.
- Create UI authentication state lazily. API-only runs must not log in through
  the UI. Bootstrap within the scenario test context, close the bootstrap browser,
  then use a fresh browser, context, and page for each UI scenario.
- Each run owns any authentication snapshot file it creates. Use a unique path;
  never log or attach the snapshot, and delete only the owned file at run end.
  Keep API request redaction enabled. Dispose process-wide browser state only
  after all scenarios complete.
- Keep worker counts in runner configuration. Before changing parallel execution,
  validate data and resource isolation and the browser factory's test-context
  contract; do not infer safe scenario parallelism from feature parallelism.

## Generated files and validation

- Edit `.feature` sources, never generated `.feature.cs` files. Use Reqnroll
  clean/rebuild to remove obsolete generated sources; keep them ignored by Git.
- Use the [BDD scope commands](../../CsharpTestAutomation.Bdd.Tests/AGENTS.md)
  and its validation requirements. Select the BDD project and filter by the
  relevant application, feature, layer, or stable-ID category. For example,
  `--filter "TestCategory=SiteApi01"` selects the project scenario shown above.
- Report which checks ran and any unavailable services or credentials. Discovery
  and builds do not prove that live scenarios pass.
- Explicit cleanup scenarios verify data deletion.
