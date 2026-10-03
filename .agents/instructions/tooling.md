# Tool Selection and Workflows

Use the smallest set of tools needed. Root `AGENTS.md` routes ordinary codebase
questions to Graphify and file discovery to FFF; consult this guide for detailed
tool choice, fallbacks, or operational workflows. Read it before external
documentation or browser work. Prefer repository evidence and static analysis
unless live behavior is needed.

| Scope | Tool | Purpose |
| --- | --- | --- |
| Code navigation | Repository Graphify skill | Symbols, relationships, callers/callees, component maps. |
| File discovery | FFF MCP | Files, directories, patterns, configs, fixtures, resources. |
| Third-party libraries | Context7 MCP | Dapper, Playwright, NLog, Polly, Bogus, NBuilder, Allure, NUnit, RestSharp APIs/errors. |
| Microsoft / .NET | Microsoft Learn MCP | C#, BCL, MSBuild, editor settings, Azure SDKs, Azure DevOps. |
| Live UI | Repository Playwright CLI skill | DOM, locators, reproduction, screenshots, traces. |
| Browser diagnostics | Chrome DevTools MCP | Network, console, cookies/storage, performance, memory. |

- Select in order: prompt context → Graphify/FFF → official docs → runtime tools.
  Avoid standard file sweeps or external searches when repository tools can locate the target.
- Known local behavior, reviews, and refactors need no external lookup when
  repository evidence establishes the answer.
- If Graphify/FFF is unavailable or cannot locate the target, use targeted `rg`
  and file reads. If a documentation MCP server is unavailable, use official
  documentation in the same scope. Report limitations; never claim tool use
  without a successful call.
- Limit Context7 and Microsoft Learn to five documentation lookups per request.
  Retain this limit for their official-documentation fallbacks. Graphify, FFF,
  Playwright, and Chrome DevTools are exempt.
- Browser escalation: Playwright CLI → trace → Chrome DevTools → source analysis.
  Validate new locators with the CLI when the app is available. Browser tools
  are development tools, not runtime test dependencies.
- Inspect only task-relevant material, reuse evidence, and stop once established.
- Before `/graphify` or Playwright CLI work, read the repository skill. For
  Graphify relationships use `path`/`explain`; use the wiki index for broad
  navigation when present. Follow its Windows reference on PowerShell.
