# Automation Framework Review

Review date: 2026-09-27. Evidence is from the current source, project files, and agent instructions. Conclusions are marked Confirmed, Likely, Possible, or Unable to verify.

## Current Architecture

```text
CsharpTestAutomation.Tests
  fixtures, page objects, typed API clients, DTOs, SQL queries
        |
        v
CsharpTestAutomation.Framework
  AppConfiguration / CoreConfiguration
  PlaywrightBrowserFactory          RestClientFactory          DapperActions
        |                                  |                        |
        v                                  v                        v
  Microsoft.Playwright                 RestSharp                  Dapper
```

`CsharpTestAutomation.Framework.Test` references the framework and does not reference the application test project. Application tests reference the framework. There is no infrastructure project and no CI pipeline file in this repository.

Dependency direction is correct: the framework does not reference application endpoints, DTOs, or `appsettings.json`. Page objects, typed clients, and SQL live in the test project.

## Review Scope

Reviewed: solution and project files, framework UI/API/DB/configuration code, application fixtures and representative tests, framework self-tests, architecture docs, `AGENTS.md`, `.agents/rules`, and `.github/instructions`.

Not executed in the initial inspection: a live application login, a database connection, or a CI run. Those items stay Unable to verify until a later check says otherwise.

## Findings

### F-01

```text
ID: F-01
Severity: High
Area: Playwright context options
Location: CsharpTestAutomation.Framework/UI/PlaywrightBrowserFactory.cs CreateBrowserContextAsync
Finding: When PlaywrightDeviceName is set, the factory replaces the context options with the cached device descriptor and then writes IgnoreHTTPSErrors and RecordVideoDir onto that shared object.
Evidence: Confirmed. The assignment uses s_playwrightInstance.Devices[deviceName] and then mutates that instance. StorageState, HttpCredentials, and BypassCSP from the original options are dropped.
Why it matters: Authenticated contexts and HTTP credentials fail under device emulation. Later contexts inherit mutated device settings.
Recommended change: Copy device fields onto a new options object. Keep storage state, credentials, CSP bypass, and video settings.
Implementation complexity: Low
Regression risk: Low. The default device name is empty, so the current suite does not take this path.
Verification required: Unit test of the options builder. No live browser required.
```

### F-02

```text
ID: F-02
Severity: High
Area: Parallel lifecycle
Location: PlaywrightBrowserFactory.DisposeAllAsync and PlaywrightBrowserFactoryTests.OneTimeTearDownAsync
Finding: DisposeAllAsync closes every browser and disposes the shared Playwright instance. A fixture OneTimeTearDown calls it. Framework.Test uses ParallelScope.Fixtures and LevelOfParallelism(2).
Evidence: Confirmed in source. No second fixture currently uses the factory, so the collision is latent.
Why it matters: A future parallel fixture loses its browser when this fixture ends, including during the explicit DisposeAllAsync test.
Recommended change: Mark the owning fixture NonParallelizable and document DisposeAllAsync as process-wide shutdown only.
Implementation complexity: Low
Regression risk: Low
Verification required: Build. The attribute does not need a live browser.
```

### F-03

```text
ID: F-03
Severity: High
Area: Authentication bootstrap reliability
Location: CsharpTestAutomation.Tests/Authentication/UiAuthenticationBootstrapper.cs
Finding: After navigation, bootstrap waits a fixed PostLoginDelayMs (default 5000) and then scans sessionStorage once. It also closes a context that PlaywrightBrowserFactory still owns, so disposal runs twice.
Evidence: Confirmed. Task.Delay(auth.PostLoginDelayMs) and context.CloseAsync() are in LoginAndCaptureAsync. The factory stores that context and closes it again in DisposeContextAsync.
Why it matters: A slow MSAL write fails the whole assembly setup, including API tests. A fast login still waits the full delay. The second close hides disposal errors.
Recommended change: Wait until the access token is present, bounded by LoginTimeoutInMs. Leave context ownership with the factory.
Implementation complexity: Low
Regression risk: Medium. Live Entra ID login was not executed here.
Verification required: Build. Live login remains NOT VERIFIED.
```

### F-04

```text
ID: F-04
Severity: Medium
Area: Database timeouts
Location: CsharpTestAutomation.Framework/DB/DapperActions.cs QueryAll
Finding: QueryAll<T> uses DbExecuteTimeoutSeconds (600). The dictionary QueryAll overload uses DbQueryTimeoutSeconds (180). Both are reads.
Evidence: Confirmed. XML docs repeat the split. Production caller is CpfQueries.SelectAllCpfs via QueryAll<T>.
Why it matters: The same read operation has two timeouts. Callers cannot predict which setting applies.
Recommended change: Use DbQueryTimeoutSeconds for both read-all overloads. Callers that need longer can pass timeoutInSeconds.
Implementation complexity: Low
Regression risk: Low. A list query that needs more than 180 seconds must pass an explicit timeout or raise DbQueryTimeoutSeconds.
Verification required: Build and existing Dapper self-tests. Those tests do not assert the timeout value.
```

### F-05

```text
ID: F-05
Severity: Medium
Area: Test data assertions
Location: CsharpTestAutomation.Tests/Tests/API/CreateCpfTests.cs
Finding: ResolveCountry indexes cpfs[0] and throws if the table is empty. The created-row check uses RequireDbData, which marks a missing created row inconclusive.
Evidence: Confirmed. RequireDbData calls Assert.Inconclusive. The create call already returned an id.
Why it matters: Missing seed data and a failed persist are different results. An index exception is a poor diagnostic for an empty table.
Recommended change: Use RequireDbData only for the country precondition. Assert that the created row exists.
Implementation complexity: Low
Regression risk: Low
Verification required: Build. Live API test remains NOT VERIFIED.
```

### F-06

```text
ID: F-06
Severity: Medium
Area: Browser scalability
Location: PlaywrightBrowserFactory.InitializeAsync
Finding: Each test launches its own browser process. Contexts are not reused from a worker-scoped browser.
Evidence: Confirmed. s_browsers is keyed by test id and LaunchAsync runs per missing key.
Why it matters: Suite growth spends time and memory on process startup. This is a cost issue, not a current functional defect.
Recommended change: Defer. Share one browser per worker and keep one context per test. Do not change lifecycle in this pass.
Implementation complexity: High
Regression risk: High
Verification required: Parallel browser tests. Not started.
```

### F-07

```text
ID: F-07
Severity: Medium
Area: Suite coupling
Location: GlobalSetupFixture and EntraIdTokenService
Finding: Every application test run bootstraps UI login when UI settings exist. API tests then read BootstrapSession.Default.Token. EntraIdTokenService can acquire a client-credentials token, but nothing calls it, and its static initializer throws if EntraIdSettings is missing.
Evidence: Confirmed. appsettings.json has no EntraIdSettings. No call site references EntraIdTokenService.
Why it matters: API tests cannot run without a browser login. The unused token service fails on first touch.
Recommended change: Lazy-load Entra ID settings so type load does not throw. Defer a second auth path until a test needs client credentials.
Implementation complexity: Low for the lazy load. High for a new auth path.
Regression risk: Low for the lazy load. No current caller.
Verification required: Build.
```

### F-08

```text
ID: F-08
Severity: Medium
Area: Shared environment data
Location: GetCpfsTests, CreateCpfTests, CpfQueries.SelectAllCpfs
Finding: List tests compare or search the shared QA table. Create uses a hard-coded team-lead id. Search picks a random row. SelectAllCpfs has no ORDER BY.
Evidence: Confirmed in the test methods and SQL.
Why it matters: A failed cleanup or an unexpected row can fail an unrelated test. Parallelism is currently 1, so cross-test races are limited.
Recommended change: Defer. Keep LevelOfParallelism at 1 until data is unique per test or assertions no longer depend on the full table.
Implementation complexity: Medium
Regression risk: Medium
Verification required: Live API and database. Not started.
```

### F-09

```text
ID: F-09
Severity: Medium
Area: Async database API
Location: DapperActions and CpfQueries
Finding: Framework rules say all I/O is async. Database helpers are synchronous and are called from async tests.
Evidence: Confirmed. DapperActions exposes only sync methods. Connections are disposed with using.
Why it matters: Calls block the test thread. There is no deadlock in the current sync Dapper path.
Recommended change: Defer async overloads. Do not add a second API until callers are migrated together.
Implementation complexity: Medium
Regression risk: Medium
Verification required: Framework DB tests and application build. Not started.
```

### F-10

```text
ID: F-10
Severity: Medium
Area: CI/CD
Location: Repository root and .github
Finding: No workflow or pipeline file is present. Rules mention Azure DevOps, but this repo does not define the host.
Evidence: Confirmed by file search. Unable to verify an external pipeline.
Why it matters: Build, analyzer, and framework self-test gates are not enforced by this repository.
Recommended change: Defer a pipeline until the CI host is confirmed. Do not invent GitHub Actions if the team uses Azure DevOps.
Implementation complexity: Medium
Regression risk: Low
Verification required: A pipeline run. Not started.
```

### F-11

```text
ID: F-11
Severity: Low
Area: Configuration documentation
Location: AppConfiguration XML docs versus README.md
Finding: XML docs say the variable is CURRENT_ENV and describe an override. Code and README use Environment, and that file replaces appsettings.json. It is not merged over it. Environment variables are applied only in that mode.
Evidence: Confirmed.
Why it matters: An agent can generate the wrong file name or assume a merge.
Recommended change: Correct the XML docs to match the code and README.
Implementation complexity: Low
Regression risk: None
Verification required: Build.
```

### F-12

```text
ID: F-12
Severity: Low
Area: AI harness drift
Location: .agents/rules/framework.md folder layout
Finding: The rule lists API/Validation/. That folder does not exist.
Evidence: Confirmed by directory listing.
Why it matters: Agents may create a parallel validation folder instead of using the existing settings validators.
Recommended change: Remove the missing folder from the rule and state the browser lifecycle constraints.
Implementation complexity: Low
Regression risk: None
Verification required: Doc review.
```

### F-13

```text
ID: F-13
Severity: Low
Area: Failure diagnostics
Location: UiTestBase.OnTearDownAsync
Finding: Context and browser disposal failures are swallowed with an empty catch.
Evidence: Confirmed.
Why it matters: A leaked browser is invisible in the log, and the original test result should still stand.
Recommended change: Log the exception. Do not rethrow.
Implementation complexity: Low
Regression risk: Low
Verification required: Build.
```

### F-14

```text
ID: F-14
Severity: Low
Area: Parallelism comment
Location: CsharpTestAutomation.Tests/AssemblyInfo.cs
Finding: The comment says fixtures run in parallel. LevelOfParallelism is 1, so they do not.
Evidence: Confirmed.
Why it matters: An agent may raise parallelism from the comment and hit shared QA data.
Recommended change: Correct the comment.
Implementation complexity: Low
Regression risk: None
Verification required: None beyond review.
```

### F-15

```text
ID: F-15
Severity: Observation
Area: Reporting
Location: PlaywrightBrowserFactory trace saving and UiTestBase
Finding: Traces and video are written to disk. They are not attached to Allure. Screenshots and browser logs are attached on Failed only.
Evidence: Confirmed.
Why it matters: Disk artifacts help locally. A CI agent that does not collect TraceDir will not see them.
Recommended change: Defer attachment of zip traces. They can make reports very large.
Implementation complexity: Medium
Regression risk: Low
Verification required: Not started.
```

### F-16

```text
ID: F-16
Severity: Observation
Area: Secrets
Location: appsettings.json, .gitignore, GlobalSetupFixture.LogAppsettingsValues
Finding: Tracked settings use PLACEHOLDER_PASSWORD. playwright/.auth/ and appsettings.local.json are ignored. Global setup logs top-level property ToString values, not nested password fields.
Evidence: Confirmed for the current models. A future record ToString that prints members would leak secrets. Likely, not current.
Why it matters: The current log path is safe. The reflection logger is brittle.
Recommended change: Defer redaction until nested values are serialized.
Implementation complexity: Low
Regression risk: Low
Verification required: Not started.
```

## Improvement Plan

### P0 — Correctness / critical risks

No confirmed Critical finding. The default device name is empty, parallelism for application tests is 1, and tracked secrets are placeholders. The high findings below are the first implementation group.

```text
Task ID: T-01
Priority: P0
Description: Copy device fields onto a new context options object. Preserve storage state, credentials, CSP bypass, and video. Do not mutate the cached device descriptor.
Reason: F-01
Dependencies: None
Affected components: PlaywrightBrowserFactory
Expected files: PlaywrightBrowserFactory.cs, a local framework test, UI architecture doc, framework rules
Implementation approach: Extract an internal options builder and unit-test it without launching a browser.
Verification: Build and PlaywrightContextOptionsTests
Status: DONE
```

```text
Task ID: T-02
Priority: P0
Description: Stop parallel fixtures from calling process-wide Playwright disposal.
Reason: F-02
Dependencies: None
Affected components: PlaywrightBrowserFactory, framework UI self-tests
Expected files: PlaywrightBrowserFactory.cs, PlaywrightBrowserFactoryTests.cs, framework rules
Implementation approach: Document DisposeAllAsync as process-wide. Mark the owning fixture NonParallelizable.
Verification: Build
Status: DONE
```

```text
Task ID: T-03
Priority: P0
Description: Wait for the MSAL access token instead of a fixed delay. Keep context disposal in the factory.
Reason: F-03
Dependencies: None
Affected components: UiAuthenticationBootstrapper
Expected files: UiAuthenticationBootstrapper.cs, UiConfigurationDTO.cs, API and UI architecture docs, test-automation rules
Implementation approach: page.WaitForFunctionAsync with LoginTimeoutInMs. Remove the manual context close.
Verification: Build passed. Live login NOT VERIFIED.
Status: CODE COMPLETE
```

### P1 — Architectural improvements

```text
Task ID: T-04
Priority: P1
Description: Use DbQueryTimeoutSeconds for both QueryAll overloads.
Reason: F-04
Dependencies: None
Affected components: DapperActions
Expected files: DapperActions.cs
Implementation approach: Change the generic overload and its XML doc. Callers can still pass timeoutInSeconds.
Verification: Build and DapperActionsTests
Status: DONE
```

```text
Task ID: T-05
Priority: P1
Description: Make created-row checks fail, and make a missing country precondition inconclusive.
Reason: F-05
Dependencies: None
Affected components: CreateCpfTests
Expected files: CreateCpfTests.cs, test-automation rules
Implementation approach: RequireDbData for the country list only. Assert the created row.
Verification: Build passed. Live API NOT VERIFIED.
Status: CODE COMPLETE
```

```text
Task ID: T-06
Priority: P1
Description: Share one browser per worker and one context per test.
Reason: F-06
Dependencies: Parallel browser characterization tests
Affected components: PlaywrightBrowserFactory, UiTestBase
Expected files: PlaywrightBrowserFactory.cs and its tests
Implementation approach: Not in this pass. Lifecycle change is larger than the confirmed defects.
Verification: Not started
Status: DEFERRED
```

```text
Task ID: T-07
Priority: P1
Description: Add a CI pipeline after the host is confirmed.
Reason: F-10
Dependencies: Team confirmation of Azure DevOps or GitHub Actions
Affected components: Repository CI
Expected files: Pipeline file, not created
Implementation approach: Do not invent a host.
Verification: Not started
Status: DEFERRED
```

### P2 — Reliability / maintainability

```text
Task ID: T-08
Priority: P2
Description: Log swallowed browser disposal failures. Correct configuration and parallelism comments. Remove the missing Validation folder from framework rules. Lazy-load Entra ID settings.
Reason: F-07, F-11, F-12, F-13, F-14
Dependencies: None
Affected components: UiTestBase, AppConfiguration, EntraIdTokenService, instructions
Expected files: those types and .agents/rules/framework.md
Implementation approach: Small local edits. No public API rename.
Verification: Build and doc review
Status: DONE
```

```text
Task ID: T-09
Priority: P2
Description: Add async Dapper methods and migrate callers.
Reason: F-09
Dependencies: Caller inventory
Affected components: DapperActions, CpfQueries
Expected files: DapperActions.cs, CpfQueries.cs, DB tests
Implementation approach: Not in this pass. A second sync/async API would duplicate the surface.
Verification: Not started
Status: DEFERRED
```

```text
Task ID: T-10
Priority: P2
Description: Reduce shared-table coupling in API tests.
Reason: F-08
Dependencies: A data strategy for team lead and country
Affected components: API tests and CPF queries
Expected files: GetCpfsTests.cs, CreateCpfTests.cs, CpfQueries.cs
Implementation approach: Not in this pass. Needs a live environment to prove uniqueness.
Verification: Not started
Status: DEFERRED
```

### P3 — Optional improvements

```text
Task ID: T-11
Priority: P3
Description: Attach traces to Allure or collect TraceDir in CI. Redact nested secrets if configuration logging starts serializing child objects.
Reason: F-15, F-16
Dependencies: CI host and a decision on report size
Affected components: Reporting
Expected files: AllureExtensions.cs, GlobalSetupFixture.cs
Implementation approach: Not started
Verification: Not started
Status: DEFERRED
```

## Implementation Progress

Done: T-01, T-02, T-04, T-08.

Code complete, live path not verified: T-03, T-05.

Deferred: T-06, T-07, T-09, T-10, T-11.

An item is DONE only after the verification listed for that task succeeds. T-03 and T-05 stay CODE COMPLETE because their live login and API paths were not executed.

## Verification Results

Executed locally on 2026-09-27. Application tests were not run. Tracked credentials are placeholders, and those tests call the QA application.

```text
dotnet build .\CsharpTestAutomation.slnx --nologo
Result: succeeded. No warnings. TreatWarningsAsErrors is enabled.

dotnet test .\CsharpTestAutomation.Framework.Test\CsharpTestAutomation.Framework.Test.csproj --filter "FullyQualifiedName~PlaywrightContextOptionsTests|FullyQualifiedName~DapperActionsTests" --nologo --no-build
Result: 8 passed, 0 failed, 0 skipped. Duration 2.5s.
Breakdown: PlaywrightContextOptionsTests 2, DapperActionsTests 6.
```

Not run:

- `CsharpTestAutomation.Tests`. Live login and API remain NOT VERIFIED.
- `PlaywrightBrowserFactoryTests`. Those tests launch a browser and navigate to an external site. T-02 is proven by the build and the `[NonParallelizable]` attribute, not by that fixture.

Compile fixes during verification, before the green build:

- `CreateContextOptions` had been left outside the factory class. It is now inside the class.
- `UiTestBase.OnTearDownAsync` had a broken `finally`. Disposal is logged and does not replace the test result.
- Playwright 1.63 `WaitForFunctionAsync` is non-generic. The bootstrap reads the token with `IJSHandle.JsonValueAsync<string>()`.

## AI Harness Review

Reviewed: root `AGENTS.md`, project `AGENTS.md` files, `.agents/rules/framework.md`, `.agents/rules/test-automation.md`, `.github/instructions`, `docs/API_TESTING_ARCHITECTURE.md`, `docs/UI_TESTING_ARCHITECTURE.md`, `docs/AGENT_SUPPORT.md`.

Context quality: Strong for layering, API client shape, page-object composition, and verification commands.

Instruction consistency: Updated in this pass. XML docs now say `Environment`, not `CURRENT_ENV`, and say the environment file replaces the base file. `API/Validation/` is removed from the framework rule. Framework rules now state process-wide disposal, device-field copy, and query versus execute timeouts. Test-automation rules say bootstrap waits for the MSAL token and that `RequireDbData` is not the assert for a created row. UI and API architecture docs match those contracts. The application assembly comment now says parallelism is 1.

Architectural guardrails: Strong. Agents are told to inspect first, keep domain code out of the framework, and avoid unrelated refactors.

AI-specific risks that remain: an agent can still add worker-scoped browser reuse without characterization tests, raise `LevelOfParallelism`, or merge environment settings over the base file. The updated rules forbid the device-options replacement, a fixed post-login delay, a parallel `DisposeAllAsync`, and `RequireDbData` for a created row.

Does this architectural change affect how an AI coding agent should work in this repository? Yes. The owning rules and architecture docs now state the lifecycle and assertion rules. No further instruction edit is required for this pass.

## Architectural Decisions

- Keep one browser process per test for now. Worker-scoped browser reuse is deferred (T-06).
- Keep UI-bootstrapped JWT as the API auth path. Do not switch API tests to `EntraIdTokenService` without a caller and configuration.
- Keep `Environment` file loading as a replacement, not a merge. Document it. Do not change the loader in this pass.
- Do not add a CI file until the host is known.

## Deferred Items

T-06, T-07, T-09, T-10, T-11. Reasons are in the plan.

## Residual Risks

- Live Entra ID login, API calls, and database access were not executed. Placeholder credentials cannot prove T-03 or T-05 at runtime.
- `QueryAll<T>` timeout changes from 600 seconds to 180 seconds unless the caller passes `timeoutInSeconds`. Existing Dapper tests do not assert the timeout value.
- Device `ViewportSize` and `ScreenSize` references are copied onto the new options object. The factory does not mutate those nested objects. A later writer that changes `Width` or `Height` on the returned options could still change the cached descriptor.
- `InitializeAsync` can still launch two browsers for the same test id if two calls overlap. Current parallel scope makes that unlikely.
- Application `LevelOfParallelism` is 1. Raising it without unique test data can collide on the shared QA database.
- No CI file means this review's build is local only.
- A pinned missing country in `CreateCpfTests.ResolveCountry` throws `InvalidOperationException`. An empty unpinned list is inconclusive. Both are preconditions. Only the created-row check is a hard assert.

## Final Review

The implemented changes match the confirmed defects and do not expand the public framework API. `CreateContextOptions` is `internal` and visible to the framework test project only.

Second pass found no unused context, no remaining `PostLoginDelayMs` read, and no empty disposal catch. The factory closes a replaced context before the next bootstrap user, so removing the extra `context.CloseAsync()` does not leak the previous context.

No new dependency. No public signature break. Deferred lifecycle, CI, async Dapper, and shared-data work stays deferred.

Open verification: live login and the create-CPF API test. Do not mark T-03 or T-05 DONE until those run against a real environment.
