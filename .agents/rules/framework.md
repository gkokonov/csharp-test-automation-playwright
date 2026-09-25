---
applyTo: "CsharpTestAutomation.Framework/**/*.cs"
trigger: always_on
description: "Rules unique to the reusable framework library."
---

# CsharpTestAutomation.Framework — Authoring Instructions

Packable class library — not an application, not a web app. Every public type is
a reusable building block, never test-specific code. Drive behaviour through
`CoreConfiguration`. `FrameworkReference Microsoft.AspNetCore.App` exists only
for `Microsoft.Extensions.Configuration` binding — no controllers, middleware,
or hosting patterns.

## Nullable

The project sets `<Nullable>warnings</Nullable>` as a transitional configuration.
This setting allows legacy unannotated files to remain oblivious without failing
the build under `TreatWarningsAsErrors=true`.

| Situation | Rule |
| --- | --- |
| New file | Standalone new files may use `#nullable enable` if written with complete null safety (no warnings). If integrating closely with unannotated legacy types, omit `#nullable enable` and use default values. |
| File already has `#nullable enable` (e.g. `RestClientFactory.cs`, `IRestClientFactory.cs`) | Full nullability is active. Use `?` for optional parameters/returns, and guard non-null inputs (`ArgumentNullException.ThrowIfNull`). |
| Editing existing oblivious file | Keep existing oblivious state. Do not add `#nullable enable` mid-file unless refactoring the entire file to be warning-free. |

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
`Logging/`, `Redaction/`, `Configuration/`, `Validation/`.
