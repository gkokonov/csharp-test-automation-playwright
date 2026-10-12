# Tool Selection and Workflows

Use the smallest set of tools needed. Root `AGENTS.md` routes ordinary codebase
questions to Codebase Memory MCP and file discovery to FFF; consult this guide for detailed
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
| Code navigation | Codebase Memory MCP | Indexed symbols, relationships, callers/callees, architecture. |
| File discovery | FFF MCP | Files, directories, patterns, configs, fixtures, resources. |
| Exact text / index gaps | Ripgrep (`rg`) | Focused literal searches and source checks when an index misses a path. |
| Third-party libraries | Context7 MCP | Dapper, Playwright, NLog, Polly, Bogus, NBuilder, Allure, NUnit, RestSharp APIs/errors. |
| Microsoft / .NET | Microsoft Learn MCP | C#, BCL, MSBuild, editor settings, Azure SDKs, Azure DevOps. |
| Live UI | Repository Playwright CLI skill | DOM, locators, reproduction, screenshots, traces. |
| Browser diagnostics | Chrome DevTools MCP | Network, console, cookies/storage, performance, memory. |
| Command output | RTK | Compact routine command output; preserve full review and machine-readable evidence. |

- Select in order: prompt context → Codebase Memory MCP/FFF → official docs → runtime tools.
  Avoid standard file sweeps or external searches when repository tools can locate the target.
- Known local behavior, reviews, and refactors need no external lookup when
  repository evidence establishes the answer.
- If Codebase Memory MCP/FFF is unavailable or cannot locate the target, use targeted `rg`
  and file reads. If a documentation MCP server is unavailable, use official
  documentation in the same scope. Report limitations; never claim tool use
  without a successful call.
- Limit Context7 and Microsoft Learn to five documentation lookups per request.
  Retain this limit for their official-documentation fallbacks. Codebase Memory MCP, FFF,
  Playwright, and Chrome DevTools are exempt.
- Browser escalation: Playwright CLI → trace → Chrome DevTools → source analysis.
  Validate new locators with the CLI when the app is available. Browser tools
  are development tools, not runtime test dependencies.
- Inspect only task-relevant material, reuse evidence, and stop once established.
- Before Playwright CLI work, read the repository Playwright CLI skill.

## Agent tool access

Verify a tool is present in the active session before relying on it; adapter
configuration alone does not prove activation. Follow the host's tool-name
format. Custom-agent access is role-scoped:

| Role | Boundary |
| --- | --- |
| Implementator | Changes only authorized files; may refresh this checkout's Codebase Memory index. |
| Reviewer | Returns findings without changing reviewed source, configuration, Git state, or application state. |
| Researcher | Answers bounded questions and does not change repository source or configuration. |

Readers use only explicitly allowlisted MCP tools. Do not use wildcards or grant
readers indexing, cache management, browser diagnostics, or other write-capable
tools. Keep search-service caches outside the checkout. Reviewers and Researchers
may create task scratch files; remove them before handoff and report any cleanup
failure. Antigravity Researcher uses built-in `search_web` and `read_url_content`
for official documentation; verify other host tool names and permissions before
binding them. Index results do not replace reading current source or checking
coverage.

For UI work, use an isolated browser session and close only sessions owned by the
task. Prefer targeted DOM snapshots and `playwright-cli find`. Store artifacts
and authentication state in ignored repository locations. Verify host-specific
tool names and permissions before binding MCP tools; use supplied MCP evidence
when a host cannot bind them. Compare a locally adapted Playwright skill with the
bundled version before replacing it.

## RTK and complete evidence

- Use the active RTK integration for routine summaries. Keep exit codes and
  failures; RTK does not grant command permissions.
- Read complete review patches, source snapshots, OCR/acceptance JSON, and Git
  inventories. Use `rtk proxy <command>` if filtering shortens evidence; do not
  parse compressed JSON. Read every chunk and preserve large raw results outside
  the checkout or in ignored artifacts.
- If expected output is empty, garbled, or conflicts with the exit code, retry
  with `rtk proxy` and report unresolved failures. `rtk playwright` summarizes
  test output; it does not interact with the browser like `playwright-cli`.

## Codebase Memory MCP

- Call `list_projects`, select this checkout by `root_path`, and pass its exact
  name to queries. Use graph search for symbols, snippets/outlines for source,
  architecture for broad navigation, and traces for relationships. Include tests
  when callers matter. Read the graph schema before Cypher queries; use `search_code`
  for literals and FFF for file discovery.
- Keep queries focused and page through truncated results. Cite source paths and
  lines, then read current source before edits; index results do not prove runtime
  behavior or full coverage.
- After code changes, use `index_status` and `check_index_coverage` on changed
  paths. Implementator may refresh a stale index with `index_repository` for this
  checkout, then repeat the focused query.
  Readers report missing/stale coverage and request a refresh.
- If MCP is unavailable, use the installed Windows CLI, preserve its shared cache,
  and check native exit codes:

```powershell
$cbm = Join-Path $env:LOCALAPPDATA 'Programs/codebase-memory-mcp/codebase-memory-mcp.exe'
& $cbm cli list_projects
```

If both MCP and CLI are unavailable, report that and use targeted `rg` and source reads.

### Windows freshness and unsupported sources

On Windows, CBM 0.11.0 may report `metadata_changed` because timestamp precision
differs. Treat this status as uncertain freshness, read affected source files
directly, and do not edit the CBM cache. A matching file timestamp or content hash
does not prove complete parsing or graph relationships.

CBM does not index Gherkin `.feature` files. Read them directly and verify bindings
through Reqnroll discovery. Do not unignore generated `.feature.cs` files to fake
index coverage; report feature-source coverage separately.
