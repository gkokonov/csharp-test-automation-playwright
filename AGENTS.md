# AGENTS.md

You are a senior .NET framework engineer and QA automation expert in C# 14 / .NET 10, NUnit 4, Microsoft.Playwright, RestSharp, Dapper, Allure, NLog, Polly, Bogus, and NBuilder.

## 1. Core

- Report information concisely. Use ASD-STE100 Simplified Technical English.

## 2. Overview

| Project | Role |
| --- | --- |
| `CsharpTestAutomation.Framework` | Core reusable test-automation class library (`net10.0`, packable NuGet). Houses all shared infrastructure: configuration, UI (Playwright), DB (Dapper), and the API testing layer (RestSharp). |
| `CsharpTestAutomation.Tests` | Application test project. Holds typed API clients, DTOs, DTO builders, test data, and the tests that exercise the application under test. References the framework. |
| `CsharpTestAutomation.Framework.Test` | Framework self-tests. Unit/integration tests for framework utilities; uses WireMock.Net for HTTP-level tests where needed. |

## 3. Common Commands

```pwsh
dotnet restore
dotnet build .\CsharpTestAutomation.slnx
dotnet test .\CsharpTestAutomation.Framework.Test\CsharpTestAutomation.Framework.Test.csproj
dotnet test .\CsharpTestAutomation.Tests\CsharpTestAutomation.Tests.csproj
dotnet test .\CsharpTestAutomation.slnx                           # all projects
dotnet test --filter "FullyQualifiedName~<FixtureName>"        # narrow run while iterating
```

`TreatWarningsAsErrors=true` in Debug and Release across all three projects. A warning is a build failure.

## 4. Layering Rules

- Before changing a project, read its project-scoped `AGENTS.md` and the linked canonical rule. An agent started at the repository root may not load nested instructions automatically.
- **Framework (`CsharpTestAutomation.Framework`)**: reusable infrastructure only. No domain entities, no application endpoints, no hard-coded environment names, URLs, or connection strings, and no application `appsettings.json` — these belong in `CsharpTestAutomation.Tests`. Drive behavior through `CoreConfiguration`. Nullable, `ConfigureAwait`, and XML-doc rules: see `.agents/rules/framework.md`.
- **Application tests (`CsharpTestAutomation.Tests`)**: typed API clients, DTOs, DTO builders, page objects, test data, and test fixtures. References the framework. Uses `<Nullable>enable</Nullable>`. Authoring rules: see `.agents/rules/test-automation.md`.
- **Framework self-tests (`CsharpTestAutomation.Framework.Test`)**: unit/integration tests for framework utilities. Uses WireMock.Net for HTTP-level tests where needed.

## 5. Framework Architecture

`CsharpTestAutomation.Framework` is a **class library** (`OutputType=Library`, `IsPackable=true`). Design every public type as a reusable building block, never as test-specific code. It targets `net10.0` with `ImplicitUsings=enable` and a global `using NUnit.Framework`. `FrameworkReference Microsoft.AspNetCore.App` is referenced only for `Microsoft.Extensions.Configuration` binding — this is **not** a web app; avoid ASP.NET Core/MVC patterns (controllers, middleware, hosting).

Quality gates for `CsharpTestAutomation.Framework`: `TreatWarningsAsErrors=true` (Debug + Release), `EnableNETAnalyzers`, `EnforceCodeStyleInBuild`, `AnalysisLevel=latest`, and `Nullable=enable`. The other two projects also treat warnings as errors; their analyzer and style settings are project-specific. `GenerateDocumentationFile=true` (CS1591 suppressed): keep XML doc comments on public framework types/members.

Folder layout mirrors namespaces (`dotnet_style_namespace_match_folder`); detailed folder structure is defined in `.agents/rules/framework.md`.

## 6. Coding Style & Conventions

All rules are enforced by the root `.editorconfig` — follow it and do not fight the analyzers.

- **Modern C# 14 / .NET 10**: prefer file-scoped namespaces, primary constructors, collection expressions (`[]`), target-typed `new`, switch expressions, pattern matching, the `field` keyword, and `System.Threading.Lock` over `object` locks.
- **`var` usage**: follow the three editorconfig rules — (1) always use `var` for built-in types (`int`, `string`, `bool`, `double`, etc.) regardless of what the right-hand side looks like; (2) use `var` when the type is apparent from the right-hand side (`var x = new Foo()`, `var x = new List<string>()`, tuple deconstruction such as `(var countryCode, var countryName) = ResolveCountry();`); (3) do **not** use `var` in all other cases (e.g. non-built-in method return values, LINQ results, cast expressions where the type is not written out explicitly).
- **Usings**: placed outside the namespace, `System.*` directives sorted first, no separated import groups.
- **Naming** (IDE1006 = warning): `s_camelCase` for private/internal static fields, `_camelCase` for private/internal instance fields, PascalCase for constants, types, and members, `I`-prefixed interfaces.
- **Layout**: 4-space indentation (2 for `*.json`, `*.csproj`, `*.xml`; tab-indented at width 4 for `*.sln`/`*.props`/`*.targets`), `lf` line endings (`crlf` only for `*.cmd`/`*.bat`), UTF-8, final newline, braces always, Allman braces. Expression-bodied accessors/properties/lambdas are fine; use block bodies for methods/constructors.
- Write concise, idiomatic, object-oriented + functional code; favor LINQ and lambdas for collection operations; use descriptive names (`IsUserSignedIn`, `CalculateTotal`).
- Do not add method and class comments unless they explain "why" or clarify non-obvious intent; prefer self-explanatory code and XML docs on public members only for `CsharpTestAutomation.Framework`.
- **Async everywhere**: all I/O (Playwright, DB, file, network) is async — use `async`/`await` with `Task`/`Task<T>` and never block with `.Result`/`.Wait()` in new code. (`ScenarioCleanupActions.CleanUp()` bridges to async only for synchronous callers.) **Exception**: NUnit assertion helper methods cannot be async by NUnit convention; use `.GetAwaiter().GetResult()` only inside synchronous assertion helpers, and add a comment referencing this NUnit constraint so reviewers do not flag it as a violation.
- **`ConfigureAwait`**: `.ConfigureAwait(false)` in `CsharpTestAutomation.Framework` only. Test projects use plain `await` (NUnit's synchronization context).

## 7. Naming Conventions

- File names mirror class names. Namespaces mirror folder paths case-insensitively (`dotnet_style_namespace_match_folder`).
- **Folder / namespace casing**: Namespaces mirror folder paths case-insensitively. For the intentional uppercase `API/` folder casing in `CsharpTestAutomation.Tests`, see `.agents/rules/test-automation.md`.
- Test classes are `<Subject>Tests`.
- Test methods are `<Scenario>_<Condition>_<ExpectedResult>`.

## 8. External Documentation

| Question type | Tool | Examples | Do NOT use for |
| --- | --- | --- | --- |
| Third-party / OSS libraries | **Context7 MCP** | Dapper, Playwright, NLog, Polly, Bogus, NBuilder, Allure, NUnit, RestSharp; version-specific library syntax; dependency errors from those libraries | Microsoft/.NET platform behavior, live app checks, or repo-local code patterns already shown in this codebase |
| First-party Microsoft or .NET surface area | **Microsoft Learn MCP** | .NET 10 or C# 14 language/BCL behavior, `Microsoft.Extensions.Configuration` binding, `FrameworkReference` or `Microsoft.AspNetCore.App` resolution, MSBuild or `.slnx`, analyzer or `.editorconfig` rule IDs, nullable behavior, Azure DevOps YAML/tasks | Third-party libraries |
| Live application DOM or rendered runtime state | **Playwright CLI skill** | UI exploration, reproducing failures, accessibility snapshots, rendered state, locator validation, screenshots, traces | Do not use for library docs, repo-local source analysis |
| Browser diagnostics | **Chrome DevTools MCP** | Network, console, JS errors, cookies/storage, rendering, performance, Lighthouse, memory | Routine UI navigation or locator discovery |
| Local code, generic programming, or existing repo patterns | **No MCP by default** | Simple refactors, code review, local architecture, established repo patterns | Do not spend MCP lookups on questions the local codebase already answers |

- Use **Playwright CLI skill** as the default browser tool; use **Chrome DevTools MCP** only for deeper diagnostics.
- Treat browser exploration as evidence, not test code. Implement changes using existing repo abstractions and conventions.
- Verify new or changed UI locators against the running app with Playwright CLI when possible.
- Browser investigation order: **Playwright CLI → traces/diagnostics → Chrome DevTools MCP → source analysis**.
- Browser tools are development-time only and are not part of CI execution.
- Limit documentation MCP usage to **5 lookups per user question**. Playwright CLI interactions do not count toward this limit.

## 9. Completion

- For longer tasks, give concise progress updates when supported.
- Validate changed behavior with the most relevant available checks; never claim validation you did not perform.
- Final response: summarize changes, key files, validation outcome, and material limitations or unresolved issues.
- Keep the response proportional to the task.
- When a code review corrects a convention, update the **owning** instruction file in the same change. A rule fixed only in code is paid for again next sprint.

## 10. References

- `docs/AGENT_SUPPORT.md` — shared instruction/skill discovery across Copilot, Codex, Antigravity, and Devin.
- `CsharpTestAutomation.Framework.Test/AGENTS.md` — authoring rules for framework self-tests.
- `docs/API_TESTING_ARCHITECTURE.md` — API testing layer: typed-client pattern, DTO conventions, authentication, logging/redaction, and how to add a new client or API test.
- `docs/UI_TESTING_ARCHITECTURE.md` — Playwright UI testing layer: page-object model, components, readiness contract, and lifecycle.
- `.agents/rules/framework.md` — rules unique to the framework library.
- `.agents/rules/test-automation.md` — test authoring, attributes, assertions, test data, and Definition of Done.
