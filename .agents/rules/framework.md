---
applyTo: "CsharpTestAutomation.Framework/**/*.cs"
description: "Rules unique to the reusable framework library."
---

# CsharpTestAutomation.Framework — Authoring Instructions

Packable class library — not an application, not a web app. Every public type is
a reusable building block, never test-specific code. Drive behaviour through
`CoreConfiguration`. `FrameworkReference Microsoft.AspNetCore.App` exists only
for `Microsoft.Extensions.Configuration` binding — no controllers, middleware,
or hosting patterns.

## Nullable

The project sets `<Nullable>warnings</Nullable>`, so files without an explicit
`#nullable enable` directive stay in oblivious mode and emit no annotation
warnings.

| Situation | Rule |
| --- | --- |
| New file | Do **not** add `#nullable enable`. Do **not** use `?` on reference types. Use default values instead. |
| File already has `#nullable enable` (currently `RestClientFactory.cs`, `IRestClientFactory.cs`) | `?` is allowed on optional parameters (`IWebProxy?`, `IAuthenticator?`). |
| Editing any existing file | Match that file's current annotation state exactly. Never add or remove the directive while modifying a file. |

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