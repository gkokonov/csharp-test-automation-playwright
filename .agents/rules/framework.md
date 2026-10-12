---
applyTo: "CsharpTestAutomation.Framework/**/*.cs"
trigger: glob
globs: "CsharpTestAutomation.Framework/**/*.cs"
description: "Rules unique to the reusable framework library."
---

# CsharpTestAutomation.Framework — Authoring Instructions

Packable class library — not an application, not a web app. Every public type is
a reusable building block, never test-specific code. Drive behaviour through
`CoreConfiguration`. `FrameworkReference Microsoft.AspNetCore.App` exists only
for `Microsoft.Extensions.Configuration` binding — no controllers, middleware,
or hosting patterns.

## Project settings

Targets `net10.0` with `OutputType=Library`, `IsPackable=true`,
`ImplicitUsings=enable`, and a global `using NUnit.Framework`.
`TreatWarningsAsErrors=true` in Debug and Release. The framework also sets
`EnableNETAnalyzers=true`, `EnforceCodeStyleInBuild=true`, and
`AnalysisLevel=latest`. Read the project file for current settings; the other
projects have their own analyzer and style settings.

## Nullable

The project sets `<Nullable>enable</Nullable>` for the entire project. Nullable
annotations and warnings are active in every source file, and warnings are
errors in Debug and Release builds.

| Situation | Rule |
| --- | --- |
| New file | Do not add a `#nullable` directive; the project setting already applies. Annotate reference types with `?` wherever `null` is legal. |
| Nullability warning | Fix the contract with a narrower type, a guard, or a correct annotation. Do not silence it with `!`, `#nullable disable`, or a pragma. |

Guard required non-null inputs, for example with `ArgumentNullException.ThrowIfNull`.

## Async

`.ConfigureAwait(false)` is required only in the framework library. Both
application test projects and the framework unit/component test project use
plain `await` by repository policy.

All I/O is async. Never block with `.Result` or `.Wait()`.

## XML Documentation

`GenerateDocumentationFile=true` with CS1591 suppressed. Keep XML doc comments
on public types and members. Never write a doc comment that merely restates the
signature — document non-obvious side effects, lifetime/ownership rules, and
required setup.

## Hard Boundaries

Never add to this project: application endpoints, domain DTOs, environment
names, connection strings, or an application `appsettings.json`. These belong in
the selected application test project: `CsharpTestAutomation.Tests` for NUnit or
`CsharpTestAutomation.Bdd.Tests` for BDD. Keep their application layers independent.

`CsharpTestAutomation.Framework.Test` contains local unit/component tests of the
framework. It is not an application test project and must not use application
endpoints, credentials, or external services.

## Folder Layout and layer references

Mirrors namespaces (`dotnet_style_namespace_match_folder`): `Common/`, `DB/`,
`UI/`, and `API/` with `Clients/`, `Authentication/`, `Interceptors/`,
`Logging/`, `Redaction/`, and `Configuration/`. There is no `API/Validation/`
folder. Service and logging validation lives on the settings types.

For API layer design, read [API architecture](../../docs/API_TESTING_ARCHITECTURE.md).
For UI layer design, read [UI architecture](../../docs/UI_TESTING_ARCHITECTURE.md).
Read only the reference for the layer under work; cross-layer work reads both.

## Browser Lifecycle

`PlaywrightBrowserFactory` holds process-wide Playwright state, keyed by test id.

- Launch one browser per test. Do not share a page or context across tests.
- `DisposeAllAsync` closes every browser and the shared Playwright instance. Call
  it only from an assembly-level teardown, or from a `[NonParallelizable]` fixture
  that owns that process. A parallel fixture must not call it.
- Diagnostics are owned per context by the factory. Collect console and page-error
  events before pages open. Create and track diagnostics only when logging or
  tracing is enabled; allocate the log queue only when log capture is enabled.
  `TraceEnabled` retains all traces; `TraceOnFailure`
  records all contexts but retains only failed-test traces. Finalize tracing before
  closing the context and attach retained traces through `AllureExtensions`.
- Evidence helpers propagate capture errors. Callers attempt artifacts independently
  and report secondary errors. Resource release attempts all owned resources, then
  reports failures; a passing test must fail on release failure.
- Device emulation copies device fields onto a new context-options object. Do not
  replace the options with `Devices[name]` and do not mutate that cached
  descriptor. Storage state, HTTP credentials, `BypassCSP`, and video settings
  still apply when `PlaywrightDeviceName` is set.

## Database Timeouts

Read timeouts use `DbQueryTimeoutSeconds`. Write timeouts use
`DbExecuteTimeoutSeconds`. Do not use the execute timeout for a read.
