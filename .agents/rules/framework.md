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

## Nullable

The project sets `<Nullable>enable</Nullable>` for the entire project. Nullable
annotations and warnings are active in every source file, and warnings are
errors in Debug and Release builds.

| Situation | Rule |
| --- | --- |
| New file | Do not add a `#nullable` directive; the project setting already applies. Annotate reference types with `?` wherever `null` is legal. |
| Nullability warning | Fix the contract with a narrower type, a guard, or a correct annotation. Do not silence it with `!`, `#nullable disable`, or a pragma. |

Use `?` for values that may be null and guard required non-null inputs (for
example, with `ArgumentNullException.ThrowIfNull`). Do not add `#nullable enable`
or `#nullable disable` directives; the project setting applies to every file.

## Async

`.ConfigureAwait(false)` is required here, and only here. The test projects use
plain `await` because NUnit provides a single-threaded synchronization context
that test code relies on.

All I/O is async. Never block with `.Result` or `.Wait()`.

## XML Documentation

`GenerateDocumentationFile=true` with CS1591 suppressed. Keep XML doc comments
on public types and members. Never write a doc comment that merely restates the
signature — document non-obvious side effects, lifetime/ownership rules, and
required setup.

## Hard Boundaries

Never add to this project: application endpoints, domain DTOs, environment
names, connection strings, or an application `appsettings.json`. These belong in
`CsharpTestAutomation.Tests`.

## Folder Layout

Mirrors namespaces (`dotnet_style_namespace_match_folder`): `Common/`, `DB/`,
`UI/`, and `API/` with `Clients/`, `Authentication/`, `Interceptors/`,
`Logging/`, `Redaction/`, and `Configuration/`. There is no `API/Validation/`
folder. Service and logging validation lives on the settings types.

## Browser Lifecycle

`PlaywrightBrowserFactory` holds process-wide Playwright state, keyed by test id.

- Launch one browser per test. Do not share a page or context across tests.
- `DisposeAllAsync` closes every browser and the shared Playwright instance. Call
  it only from an assembly-level teardown, or from a `[NonParallelizable]` fixture
  that owns that process. A parallel fixture must not call it.
- Device emulation copies device fields onto a new context-options object. Do not
  replace the options with `Devices[name]` and do not mutate that cached
  descriptor. Storage state, HTTP credentials, `BypassCSP`, and video settings
  still apply when `PlaywrightDeviceName` is set.

## Database Timeouts

Read timeouts use `DbQueryTimeoutSeconds`. Write timeouts use
`DbExecuteTimeoutSeconds`. Do not use the execute timeout for a read.
