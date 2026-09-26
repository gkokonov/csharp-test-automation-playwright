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
`Logging/`, `Redaction/`, `Configuration/`, `Validation/`.
