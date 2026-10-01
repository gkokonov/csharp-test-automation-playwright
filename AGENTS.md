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

## 6. Coding Standards

- The root `.editorconfig` is authoritative for mechanical formatting, naming, using placement, whitespace, and analyzer-backed C# style. Do not duplicate or override those rules in prose.
- Read `.agents/rules/csharp.md` when editing C# code.
- Apply scoped rules when working in `CsharpTestAutomation.Framework/**` or `CsharpTestAutomation.Tests/**`.
- Prefer existing repository patterns. Do not add abstractions, suppress diagnostics, or change style settings only to make a change pass validation.
- Before completing code changes, run the relevant formatter/analyzer, build, and focused tests. Fix diagnostics introduced by the change.

If documentation conflicts with `.editorconfig`, `.editorconfig` wins for mechanical formatting and analyzer-backed style. Treat other conflicts as documentation defects and update the owning rule file when the convention changes.

## 7. C# and Test Naming References

- File names normally mirror their primary types, and namespaces follow folder paths. See `.agents/rules/csharp.md` for semantic conventions.
- Test classes are `<Subject>Tests`; test methods are `<Scenario>_<Condition>_<ExpectedResult>`.
- The intentional uppercase `API/` folder exception is documented in `.agents/rules/test-automation.md`.

## 8. External Documentation & Tooling

| Scope | Tool | Key Use Cases | Do NOT Use For |
| :--- | :--- | :--- | :--- |
| **Code Understanding & Call Paths** | **CodeGraph MCP/CLI** | Finding symbol definitions, usages, and call paths in one call; blast-radius of a change | Repos with no `.codegraph/` directory, or non-code (docs, config prose) questions |
| **File Search & Indexing** | **FFF MCP** | Locating files, pattern searching, directory trees, file discovery | Web search or non-file queries |
| **Architecture & History** | **Codebase Memory MCP / Skill** | Stored architectural decisions, domain entities, project structure patterns | Fresh file searches (use FFF) or inline refactoring |
| **Third-Party / OSS Libraries** | **Context7 MCP** | Dapper, Playwright, NLog, Polly, Bogus, NBuilder, Allure, NUnit, RestSharp; library syntax & errors | First-party .NET, live UI, or existing repo code |
| **1st-Party Microsoft / .NET** | **Microsoft Learn MCP** | .NET/C# BCL behavior, MSBuild, `.editorconfig`, ASP.NET Core, Azure DevOps | Third-party libraries |
| **Live App State / UI** | **Playwright CLI skill** | UI exploration, failure repros, visual DOM inspection, locator validation, trace analysis | Library docs or static repo analysis |
| **Browser Diagnostics** | **Chrome DevTools MCP** | Console payloads, network traffic, cookies/storage, performance, memory | Standard UI navigation or locator discovery |
| **Local Code & Patterns** | **No MCP** | Code reviews, inline refactoring, local logic within open files | Unnecessary lookups when local code provides the answer |

- **Default Tools:** Reach for **CodeGraph** before grep/find or reading files to understand or locate code; use **FFF MCP** for plain file discovery and **Playwright CLI** for UI exploration (escalation: `Playwright CLI` → `Traces` → `Chrome DevTools MCP` → `Source Analysis`).
- **Execution Rules:** Check **Codebase Memory** before large code sweeps. Validate new locators against the running app with Playwright CLI. Browser tools are dev-time only, not for CI execution.
- **Lookup Limit:** Max **5 external doc lookups** per request (CodeGraph, FFF, Codebase Memory, Chrome DevTools MCP, and Playwright CLI are exempt).

<!-- CODEGRAPH_START -->
### CodeGraph

In repositories indexed by CodeGraph (a `.codegraph/` directory exists at the repo root), reach for it BEFORE grep/find or reading files when you need to understand or locate code:

- **MCP tool** (when available): `codegraph_explore` answers most code questions in one call — the relevant symbols' verbatim source plus the call paths between them, including dynamic-dispatch hops grep can't follow. Name a file or symbol in the query to read its current line-numbered source. If it's listed but deferred, load it by name via tool search.
- **Shell** (always works): `codegraph explore "<symbol names or question>"` prints the same output.

If there is no `.codegraph/` directory, skip CodeGraph entirely — indexing is the user's decision.
<!-- CODEGRAPH_END -->

## 9. Completion

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
