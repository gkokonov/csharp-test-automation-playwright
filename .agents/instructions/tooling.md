# Tool Selection and Workflows

Use the smallest set of tools needed. Root `AGENTS.md` routes ordinary codebase
questions to Graphify and file discovery to FFF; consult this guide for detailed
tool choice, fallbacks, or operational workflows. Read it before external
documentation or browser work. Prefer repository evidence and static analysis
unless live behavior is needed.

## Guidance reading checklist

Use this checklist at task start and when the scope changes. Keep its record in
the working context; do not create a repository file for each task.

1. **Select the scope.** Use root `AGENTS.md`, then each affected project's
   entry point. Follow only the rules, architecture references, feature specs,
   and skills required for the task. Do not omit required guidance to save output.
2. **Record what is available.** Track the path, applicable scope or sections,
   key constraints, and read status: complete, pending, or truncated. Count
   supplied instructions as available; tool availability does not prove a skill
   or instruction file was read.
3. **Reuse complete reads.** Do not read unchanged guidance again on each
   follow-up. Read it again when it changes, the scope adds unread sections, or
   the required content is no longer available in context.
4. **Select sections before long reads.** Find headings with
   `rg -n '^##|^###' <file>`, then read the applicable sections in bounded chunks.
   Keep short entry points complete. Follow links from the selected sections.
5. **Limit output before the call.** Batch independent short reads only when
   they fit the output budget. Read instructions, source code, schema details,
   and validation results in separate groups. Request more output or smaller
   chunks when needed; do not print whole schemas or broad file collections.
6. **Resolve truncation.** Treat omitted content as unread. Locate and read the
   missing required sections before relying on the document or claiming a
   complete review. Do not repeat the same oversized read.
7. **Carry the record forward.** Include applicable paths, key constraints,
   complete sections, and pending reads in a compaction summary or authorized
   subagent handoff. After compaction, use that record and read any required
   content that is missing; do not restart all instruction discovery.

## Tool selection

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
