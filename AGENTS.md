# AGENTS.md

You are a senior .NET framework engineer and QA automation expert in C# 14 / .NET 10,
NUnit, Microsoft.Playwright, RestSharp, Dapper, Allure, NLog, Polly, Bogus, and NBuilder.
Report concisely. Use ASD-STE100 Simplified Technical English.

## Repository boundaries

| Project | Responsibility |
| --- | --- |
| `CsharpTestAutomation.Framework` | Reusable, packable test infrastructure; configure through `CoreConfiguration`. |
| `CsharpTestAutomation.Tests` | Application clients, DTOs, builders, page objects, steps, data, settings, and tests. |
| `CsharpTestAutomation.Framework.Test` | Deterministic framework unit and local integration tests. |

- Keep application endpoints, domain entities, environment names, URLs, connection
  strings, and application settings out of the framework. It is a library, not a web app.
- Framework self-tests must not use application endpoints, credentials, or external services.
- Read the relevant project `AGENTS.md` and its linked rules before edits or reviews,
  including when the session starts at the root.

## Read by task

Read only the guidance needed for the task; follow its applicable links.
Use the [guidance reading checklist](.agents/instructions/tooling.md#guidance-reading-checklist)
before instruction reads. Reuse guidance already available in the session.

| Task | Required guidance |
| --- | --- |
| C# edits or reviews | [C# rules](.agents/rules/csharp.md) and the relevant project `AGENTS.md`. |
| Framework work | [Framework scope](CsharpTestAutomation.Framework/AGENTS.md). |
| Application work | [Application scope](CsharpTestAutomation.Tests/AGENTS.md); it routes API, UI, and supporting helpers. |
| Framework self-tests | [Self-test scope](CsharpTestAutomation.Framework.Test/AGENTS.md). |
| Code review | [Review instructions](.agents/instructions/code-review-instructions.md); read-only unless fixes are requested. |
| Commit message or authorized commit | [Commit instructions](.agents/instructions/commit-instructions.md); these do not authorize a commit. |
| Detailed tool selection or workflows | [Tooling instructions](.agents/instructions/tooling.md) for fallbacks and operational details; read it before external docs or browser work. |
| Instruction or host setup changes | [Agent support](docs/AGENT_SUPPORT.md). |

Use API/UI architecture references linked by the relevant scope for changes to
those layers. Cross-layer tasks read both sets of rules. Load application feature
specifications only for the feature under work.

## Quality and validation

- `.editorconfig` owns mechanical formatting, naming, using placement, whitespace,
  and analyzer-backed style. Do not duplicate or override it in prose.
- Prefer existing patterns. Do not add abstractions, suppress diagnostics, or
  change style settings only to pass validation.
- All projects treat warnings as errors in Debug and Release.
- For C# changes, verify formatting/analyzers, build the affected scope, and run
  focused tests. Fix diagnostics introduced by the change; use project commands below.
- For instruction-only changes, run the size/link check in the support guide and
  `git diff --check`; no .NET build is required.
- When a convention changes, update its owning instruction file in the same change.
  Resolve prose conflicts there; `.editorconfig` wins for mechanical style.
- Report changes, key files, checks performed, and material limitations. Never
  claim validation or tool use that did not occur.

## Essential commands

Run from the repository root. Project entry points contain focused build/test commands.

```pwsh
dotnet restore
dotnet build .\CsharpTestAutomation.slnx
dotnet test .\CsharpTestAutomation.Tests\CsharpTestAutomation.Tests.csproj
dotnet test --filter "FullyQualifiedName~<FixtureName>"
```

## Navigation and Codebase Memory MCP

- Prefer prompt/repository evidence and static analysis. Use FFF for file discovery;
  if it cannot locate the target or is unavailable, use targeted `rg` and file reads.
- For codebase questions, use Codebase Memory MCP. Select this checkout with
  `list_projects`; query its index before broad source reads. See the
  [tooling instructions](.agents/instructions/tooling.md#codebase-memory-mcp).
- Use `search_graph` for symbols, `trace_path` for relationships, and
  `get_architecture` for broad navigation. Read the graph schema before Cypher queries.
- After code changes, check index freshness and changed-path coverage. Refresh
  with `index_repository` if needed; a connected watcher can update the index.
