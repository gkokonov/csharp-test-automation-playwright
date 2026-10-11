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

The host adapters grant tools; this file owns when to use them. Check the actual
session tool list and permissions before use. An installed server or a tool name
in an adapter does not prove activation. Use raw MCP tool names in Codex
`enabled_tools`; VS Code uses `<server>/<tool>`. Context7 names contain hyphens:
`resolve-library-id` and `query-docs`.

| Capability | Implementator | Reviewer | Researcher |
| --- | --- | --- | --- |
| FFF and Codebase Memory inspection | Available. | Available with verified restrictions. | Use only when the bounded question needs repository context. |
| Context7 and Microsoft Learn | Use the documentation routes above. | Verify a relevant API or contract. | Primary documentation tools; use official version-specific sources. |
| Ripgrep and deterministic OCR/Git | Existing command access. | Only in a verified read-only OS sandbox; otherwise request operator evidence. | Use permitted search tools or supplied source; do not add a shell for research. |
| Playwright CLI and Chrome DevTools | Authorized UI work; follow browser escalation above. | Inspect supplied snapshots, traces, and logs. | Use documentation tools and static URL fetches. |
| RTK | Routine command summaries. | Preserve full patches and source. | No extra command access. |

Reader MCP allowlists contain only FFF `find_files`, `grep`, `multi_grep`;
Codebase Memory `list_projects`, `search_graph`, `search_code`, `trace_path`,
`get_architecture`, `get_code_snippet`, `get_file_outline`, `get_graph_schema`,
`query_graph`, `index_status`, `check_index_coverage`, `detect_changes`,
`compare_graphs`; Context7's two tools; and Microsoft Learn's documentation search,
fetch, and code-sample search tools. Keep repository indexing, project deletion,
trace ingestion, and ADR updates unavailable to readers. Implementator may also
use `index_repository` for this checkout. Allowlist new tool names deliberately;
do not use MCP wildcards for reader roles.

Read-only means no changes to repository source, configuration, Git state, or
application state. Search services may maintain their own normal caches and
indexes outside the checkout. This exception grants no reader management tools
and no permission to install tools, change cache settings, or refresh an index
explicitly. If service writes cannot stay outside the checkout, keep it disabled
for readers and use supplied evidence. Check index coverage and read current
source; an index result alone does not establish complete review coverage.

Chrome DevTools stays disabled for readers: a filesystem sandbox cannot prevent
browser or remote application changes. Implementator uses its selected diagnostic
tools only when the UI task needs them. Prefer targeted DOM snapshots and
`playwright-cli find` over repeated whole-page captures. Use an isolated browser
session for the task and close only sessions owned by that task. Keep artifacts
and authentication state in the repository's ignored locations. A locally adapted
Playwright skill may differ from the bundled skill because of its Windows guidance;
compare the content before replacing it.

Antigravity adapters use documented built-in tool names. Its Researcher has
`search_web` and `read_url_content` for official documentation fallback. Exact MCP
names and role filtering must be verified in `/mcp` and the agent's actual tool
list before binding MCP tools there. Do not copy SDK-only names or permission
selectors into a custom agent's tool list. Supply MCP evidence from the parent
when that binding is unavailable. This fallback also applies to other hosts.

## RTK and complete evidence

- Reuse the active host RTK integration; avoid wrapping an already rewritten
  command twice. For a host without integration, use the installed RTK wrappers
  for routine build/test/status summaries through existing command permissions.
- Keep exit codes and failures. RTK does not grant command permissions or select
  a model. Its savings estimates do not establish billed model savings.
- Read complete review patches, source snapshots, OCR JSON, and acceptance JSON.
  Bypass compression with `rtk proxy <command>` when automatic filtering would
  shorten this evidence. Do not pass compressed JSON to a parser. Preserve large
  raw results outside the checkout or in ignored artifacts and read every chunk.
- If output is empty when expected, conflicts with the exit code, or is garbled,
  retry through `rtk proxy` and report unresolved failures. A compact status or
  diff summary does not replace the complete Git/OCR inventory or patch.
- `rtk playwright` summarizes test-run output; it is not the `playwright-cli`
  browser interaction command. Local tools do not call an agent model for these
  operations; reading their output still uses conversation context.

## Codebase Memory MCP

- Call `list_projects` and select the entry whose `root_path` matches this
  checkout. Pass its exact name as `project` in subsequent calls.
- Use `search_graph` for symbols, `get_code_snippet` for a returned qualified
  name, and `get_file_outline` for a known file. Use `get_architecture` for broad
  navigation and `trace_path` for relationships. Set `include_tests=true` when
  test callers matter; traces omit tests by default.
- Before `query_graph`, call `get_graph_schema`; request `diagnostics=full` when
  property names are needed. Check exposed tool schemas before passing arguments.
- Keep queries focused. Page truncated results with the returned offsets or
  cursors. Use `search_code` for literals in indexed files and FFF for discovery.
- Cite returned source paths and lines. Read source before edits; indexed
  relationships do not prove runtime behavior or complete coverage.
- After code changes, use `index_status` and `check_index_coverage` for changed
  paths. Auto-index covers new projects; the watcher refreshes existing indexes
  while a client is connected. Use `index_repository` with this checkout's
  absolute `repo_path` if stale or missing, then repeat the focused query.
  Readers report stale/missing coverage and ask Implementator or the operator to
  refresh; the reader allowlist does not grant `index_repository`.
- If this session lacks CBM MCP tools, use the installed Windows CLI. Keep the
  shared account cache and executable; check native command exit codes.

```powershell
$cbmExe = Join-Path $env:LOCALAPPDATA 'Programs/codebase-memory-mcp/codebase-memory-mcp.exe'
& $cbmExe cli list_projects
& $cbmExe cli get_graph_schema --project '<name from list_projects>'
& $cbmExe cli search_graph --project '<name from list_projects>' --name-pattern 'CoreConfiguration' --limit 5
```

To refresh this checkout:

```powershell
& $cbmExe cli index_repository --repo-path (Get-Location).Path
if ($LASTEXITCODE -ne 0) { throw 'CBM indexing failed.' }
```

If CBM is unavailable, report the failure and use targeted `rg` and source reads.
