# AGENTS.md

You are a senior .NET framework engineer and QA automation expert in C# 14 / .NET 10, NUnit, Microsoft.Playwright, RestSharp, Dapper, Allure, NLog, Polly, Bogus, and NBuilder.

## 1. Core

- Report information concisely. Use ASD-STE100 Simplified Technical English.
- Read this root `AGENTS.md` before repository work. Keep its uppercase filename for agent discovery. If a session starts in a project directory, also read that project's `AGENTS.md`.
- For commit messages or an authorized commit, read `.agents/instructions/commit-instructions.md`. These conventions do not authorize a commit.
- For code review, read `.agents/instructions/code-review-instructions.md`. Review is read-only unless the user also requests fixes. Apply the relevant project instructions and canonical rules to the reviewed code.

## 2. Overview

| Project | Role |
| --- | --- |
| `CsharpTestAutomation.Framework` | Core reusable test-automation class library (`net10.0`, packable NuGet). Houses all shared infrastructure: configuration, UI (Playwright), DB (Dapper), and the API testing layer (RestSharp). |
| `CsharpTestAutomation.Tests` | Application test project. Holds typed API clients, DTOs, DTO builders, test data, reusable API/UI steps, and the tests that exercise the application under test. References the framework. |
| `CsharpTestAutomation.Framework.Test` | Framework self-tests. Unit/integration tests for framework utilities; uses WireMock.Net for HTTP-level tests where needed. |

## 3. Common Commands

```pwsh
dotnet restore
dotnet build .\CsharpTestAutomation.slnx
dotnet test .\CsharpTestAutomation.Framework.Test\CsharpTestAutomation.Framework.Test.csproj
dotnet test .\CsharpTestAutomation.Tests\CsharpTestAutomation.Tests.csproj
dotnet test .\CsharpTestAutomation.slnx                         # all projects
dotnet test --filter "FullyQualifiedName~<FixtureName>"        # narrow run while iterating
```

`TreatWarningsAsErrors=true` in Debug and Release across all three projects. A warning is a build failure.

## 4. Layering Rules

- Before changing a project, read its project-scoped `AGENTS.md` and the linked canonical rule. An agent started at the repository root may not load nested instructions automatically.
- **Framework (`CsharpTestAutomation.Framework`)**: reusable infrastructure only. No domain entities, no application endpoints, no hard-coded environment names, URLs, or connection strings, and no application `appsettings.json` — these belong in `CsharpTestAutomation.Tests`. Drive behavior through `CoreConfiguration`. Nullable, `ConfigureAwait`, and XML-doc rules: see `.agents/rules/framework.md`.
- **Application tests (`CsharpTestAutomation.Tests`)**: typed API clients, DTOs, DTO builders, page objects, test data, reusable steps in `Steps/API/<App>/` or `Steps/UI/<App>/`, and test fixtures. References the framework. Uses `<Nullable>enable</Nullable>`. Authoring rules: see `.agents/rules/test-automation.md`.
- **Framework self-tests (`CsharpTestAutomation.Framework.Test`)**: unit/integration tests for framework utilities. Uses WireMock.Net for HTTP-level tests where needed.

## 5. Framework Architecture

`CsharpTestAutomation.Framework` is a **class library** (`OutputType=Library`, `IsPackable=true`). Design every public type as a reusable building block, never as test-specific code. It targets `net10.0` with `ImplicitUsings=enable` and a global `using NUnit.Framework`. `FrameworkReference Microsoft.AspNetCore.App` is referenced only for `Microsoft.Extensions.Configuration` binding — this is **not** a web app; avoid ASP.NET Core/MVC patterns (controllers, middleware, hosting).

Quality gates for `CsharpTestAutomation.Framework`: `TreatWarningsAsErrors=true`, `EnableNETAnalyzers`, `EnforceCodeStyleInBuild`, `AnalysisLevel=latest`, and `Nullable=enable`. The other two projects also treat warnings as errors; their analyzer and style settings are project-specific. `GenerateDocumentationFile=true` (CS1591 suppressed): keep XML doc comments on public framework types/members.

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
- Test classes are `<Subject>Tests`; test methods use
  `Verify_[ExpectedBehavior]_When_[StateUnderTest]` when state context helps
  (e.g. `Verify_ArgumentNullExceptionThrown_When_IdIsNull`). Use the shorter
  `Verify_[ExpectedBehavior]` form when it remains clear (e.g.
  `Verify_MaxPasswordLength_Is32`, `Verify_DatabaseConnection_StaysOpen`).
- Prefer AwesomeAssertions for value assertions, including multiple independent
  assertions inside `using (new AssertionScope())`. Use NUnit's
  `Assert.EnterMultipleScope()` for multiple NUnit assertions. Playwright
  `Expect` failures throw `PlaywrightException` and stop that scope, so do not
  use it to collect multiple Playwright failures. A null-conditional assertion
  skips the assertion when the target is null; use it only when null is an
  accepted state (e.g. `device?.StatusCode.Should().Be(HttpStatusCode.NotFound);`).
  Assert required targets are not null before checking their members.

## 8. External Documentation & Tooling

Use the **smallest set of tools needed**. Prefer repository evidence over external documentation, and static analysis over runtime diagnostics unless the task requires live behavior.

| Scope | Tool | Use For |
| :--- | :--- | :--- |
| **Code Navigation & Graph** | **Graphify** | Workspace dependency graphs, structural analysis, symbols, callers/callees, and component mapping. |
| **File Discovery** | **FFF MCP** | Files, directories, filename/pattern searches, configs, fixtures, resources. |
| **Third-Party / OSS** | **Context7 MCP** | APIs, syntax, configuration, and errors for Dapper, Playwright, NLog, Polly, Bogus, NBuilder, Allure, NUnit, RestSharp. |
| **Microsoft / .NET** | **Microsoft Learn MCP** | .NET/C# BCL, MSBuild, `.editorconfig`, ASP.NET Core, Azure SDKs, Azure DevOps. |
| **Live UI** | **Playwright CLI skill** | UI exploration, repros, DOM inspection, locator validation, screenshots, traces. |
| **Browser Diagnostics** | **Chrome DevTools MCP** | Network, console, cookies/storage, performance, memory. |
| **Known Local Code** | **No external lookup** | Code review, refactoring, or behavior already established by repository evidence. |

- **Selection:** Existing prompt context → Graphify / FFF MCP → Official docs → Runtime tools. Do not use standard file sweeps or preemptive external searches when Graphify or FFF MCP can locate the target.
- **Unavailable tools:** If Graphify or FFF is unavailable or cannot locate the target, use targeted `rg` searches and file reads. If a documentation MCP server is unavailable, use official documentation for the same scope. Report the limitation; do not claim a tool was used when it was not.
- **Browser escalation:** `Playwright CLI → Trace → Chrome DevTools MCP → Source Analysis`. Validate new locators with Playwright CLI when the app is available. Browser tools are dev-time only.
- **Context discipline:** Inspect only what is needed for the task. Reuse gathered evidence and stop once the answer is established.
- **Lookup limit:** Max **5 documentation lookups** per request across Context7 and Microsoft Learn. Graphify, FFF, Playwright, and Chrome DevTools are exempt.

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

## graphify

This project has a knowledge graph at graphify-out/ with god nodes, community structure, and cross-file relationships.

When the user types `/graphify`, use the installed graphify skill or instructions before doing anything else.

Rules:

- For codebase questions, first run `graphify query "<question>"` when graphify-out/graph.json exists. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- Dirty graphify-out/ files are expected after hooks or incremental updates; dirty graph files are not a reason to skip graphify. Only skip graphify if the task is about stale or incorrect graph output, or the user explicitly says not to use it.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing.
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).
