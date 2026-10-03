# Coding Agent Support

This repository uses shared `AGENTS.md` instructions and Agent Skills. Avoid provider-specific copies of the same rules. Keep host adapters small and add them only when discovery tests show that a host needs them.

## Discovery Matrix

| Host | Shared repository instructions | Scoped guidance | Skills |
| --- | --- | --- | --- |
| GitHub Copilot (IDE and cloud) | Root `AGENTS.md` | `.github/instructions/*.instructions.md` path pointers and project `AGENTS.md` | `.agents/skills/` |
| OpenAI Codex (desktop, IDE, and CLI) | Root-to-working-directory `AGENTS.md` chain | Project `AGENTS.md`; read linked canonical rules | `.agents/skills/` |
| Google Antigravity (IDE and CLI) | Root and directory-scoped `AGENTS.md` | `.agents/rules/*.md` also loads by matching `trigger`/`globs` | `.agents/skills/` |

Project `AGENTS.md` files are short discovery entry points. Copilot's
`.github/instructions/` adapters point to canonical rules; Antigravity uses
their `trigger`/`globs` metadata. Do not copy policy into host adapters.

## Instruction ownership and routing

The root contains repository-wide boundaries, validation, commands, and task
routes. Target 60–80 physical lines, with a hard maximum of 100 including blank
lines. Aim for 5 KiB or less; byte size is reported as a context-size check,
not a second hard limit. Do not hide prose in long lines to meet the line cap.

| Guidance | Owner and activation |
| --- | --- |
| Mechanical C# style | `.editorconfig`; prose links to it instead of restating settings. |
| Semantic C# rules | `.agents/rules/csharp.md`; C# edits and reviews in all projects. |
| Framework policy | `.agents/rules/framework.md`; framework project work. |
| Shared application rules | `.agents/rules/test-automation.md`; application C# work, applied by responsibility. |
| API policy | `.agents/rules/api-testing.md`; API clients, DTOs, builders, fixtures, `ApiTestBase`, API steps, and API helpers. |
| UI policy | `.agents/rules/ui-testing.md`; page objects, components, fixtures, `UiTestBase`, UI steps, and UI helpers. |
| Framework self-tests | `CsharpTestAutomation.Framework.Test/AGENTS.md`; local NUnit, WireMock, and SQLite behavior. |
| Detailed tool selection and workflows | `.agents/instructions/tooling.md`; fallbacks and operational details, plus external-doc and browser tasks. Root `AGENTS.md` routes ordinary code navigation and file discovery. |
| Review and commit workflows | `.agents/instructions/`; only for the requested workflow. |
| Layer architecture and feature specifications | `docs/`; read for the layer/feature under work. |

Keep root-started tasks explicit: read each affected project's entry point and
applicable canonical rules. A UI scenario that uses API setup reads both layer
rules; a page-object-only edit need not load API rules. Helpers route by what
they do, even when a path glob cannot identify that responsibility.

API and UI globs include their client/page, fixture, base-fixture, and step
paths. Keep Copilot adapter `applyTo` patterns and Antigravity canonical `globs`
consistent. Do not add a catch-all rule that loads both layer documents.

When moving a rule, update its incoming links and read trigger in the same
change. Keep shared ownership/cleanup policy in the test rule; device-specific
identifiers, prerequisites, and delete outcomes belong in the
[Device specification](netbox-automation/03-device-management-spec.md).
Maintain the short root Graphify route; generated installer blocks must also
pass the root size check.

## Host Limits and Activation

- Codex combines its active instruction chain up to `project_doc_max_bytes`, which defaults to 32 KiB. Keep root and nested instructions concise.
- Antigravity scans immediate `.agents/rules/*.md` files. Each needs YAML frontmatter with a valid `trigger`; glob rules need `globs`. Nested rule directories need explicit registration.
- Agent Skills use the shared `SKILL.md` format. Keep each skill focused and describe both its purpose and when it should activate.
- Copilot's `.github/instructions/*.instructions.md` files are available for additional path-specific activation. Use them only for behavior that cannot be expressed or discovered through `AGENTS.md`.

## Codex on Windows

### Prerequisites and activation

Use a local Windows Codex client with project configuration and Agent Skills
support. Open this checkout as the workspace, mark the project trusted in the
client, then start a fresh session. The desktop app, IDE extension, and CLI use
the same Codex host configuration. An untrusted project does not load its
`.codex/config.toml`. Do not add trust entries or change permissions in a
shared repository file.

The root `AGENTS.md` is the entry point. Codex automatically builds the
root-to-working-directory instruction chain; when a task starts at the root
and touches a project, the root instructions require reading that project's
`AGENTS.md` and its linked canonical rules. Codex does not apply the
Antigravity `trigger`/`globs` frontmatter in `.agents/rules/` through this setup;
the explicit reading requirements provide that routing. Keep the uppercase
filename. Check for `AGENTS.override.md` if the expected instructions are absent.

Use the existing repository skills directly from `.agents/skills/`. Do not
copy them to a personal skill directory. A same-named personal skill can also
appear in the selector; select the repository path when checking this harness.
Graphify includes a [Codex/PowerShell reference](../.agents/skills/graphify/references/codex-windows.md).
The software-design skill retains its existing `agents/openai.yaml` metadata.

MCP prerequisites are:

| Server | Local requirement | Codex configuration |
| --- | --- | --- |
| Context7 | `CONTEXT7_API_KEY` in the environment inherited by the Codex host; network access | HTTP with an environment-supplied bearer token |
| Microsoft Learn | Network access | HTTP; retains `maxTokenBudget=5000` |
| Chrome DevTools | Node.js/npm with `npx` on `PATH`; Chrome available; npm access on first launch | `cmd.exe /d /c` launches the headless MCP command; Windows path variables are forwarded |
| FFF | `fff-mcp.exe` installed under `LOCALAPPDATA/fff-mcp/bin` | PowerShell resolves the path from the forwarded `LOCALAPPDATA` variable |

Graphify also needs its installed Python environment. Browser tasks need the
installed Playwright CLI or the local `npx --no-install playwright cli` command.
These tools are for agent development work; they do not change test dependencies.

Set credentials through the local host environment before starting Codex. If
an environment variable changes, restart the desktop app or IDE host so its
child processes receive the new value. Never store key values in the repository.

### Configuration boundaries and fallbacks

`.codex/config.toml` defines the four optional MCP servers for local Windows
Codex clients. It does not choose a model or change sandbox, permission, or
approval settings. A server connection failure must not prevent the session
from starting. Report missing tools or credentials explicitly.

Keep documentation servers on Streamable HTTP to avoid local Node.js startup.
Forward only the Windows path variables needed by local servers with `env_vars`;
do not embed user profile or installation paths. FFF uses PowerShell without
profiles to resolve its executable from `LOCALAPPDATA`. Chrome uses `cmd.exe /d`
to bypass shell AutoRun commands and allows 60 seconds for npm startup.
Keep the default tool timeout unless a measured tool operation needs more time.
Chrome uses `npx --prefer-offline` to reuse cached package metadata without
freshness checks; missing cache data still requires npm access. The `@latest` tag
can therefore use an older cached version. To refresh it, run
`npx --prefer-online -y chrome-devtools-mcp@latest --version`, then restart the
client and repeat connection checks. For repeatable local runs, pin a tested
version in the affected adapters and repeat checks when upgrading.

Keep `.vscode/mcp.json` for Copilot and `.agents/mcp_config.json` for the
existing Antigravity setup. Their transports and schemas can differ; Codex
uses the TOML adapter. When changing a shared server endpoint or launch flags,
check the affected adapters for consistency.

If FFF or Graphify is unavailable, use targeted `rg` searches and file reads.
If a documentation MCP server is unavailable, use official documentation in
the same tool scope and retain the repository lookup limit. Do not claim a
connection or tool call succeeded without checking it.

Commit and review tasks are routed by root `AGENTS.md` to the shared files in
`.agents/instructions/`. The review instructions remain read-only unless the
user requests fixes. Copilot's review setting points to the same neutral file.
The empty `.agents/hooks.json` and `.agents/prompts/` contain no workflows to
port. Do not create Codex hooks or plugins for these placeholders.

### Connection and behavior checks

From a trusted checkout, run:

```powershell
codex mcp list
codex mcp get context7
codex mcp get microsoft-learn
codex mcp get chrome-devtools
codex mcp get fff
```

These commands inspect configuration; they do not prove a server connected.
In a fresh client session, use `/mcp` where supported to inspect active server
status. Request one small tool call from each available server: resolve the
NUnit library with Context7, search Microsoft Learn for `dotnet build`, list
Chrome pages, and find `AGENTS.md` with FFF. Report connection failures separately.

Use the discovery prompt below at the root and in all three project
directories. Confirm the response uses actual loaded/read files, identifies
all three repository skills, and applies the correct canonical rules. Also
check a root-started task that targets a project; it must explicitly read the
project instructions. Instruction files alone do not prove activation.

To check conditional routing, request a commit message for a documentation
change without a work item. Expect `docs(automation): ...`, at most 72
characters, no invented work item, and no commit. Request a review without
fixes, then compare `git diff` before and after; the review must not modify
files. Configuration and instruction-only changes do not need a .NET build.

## Discovery Check

For each supported host and surface, start a fresh session at the repository root and ask:

> List the repository instruction files and skills you loaded. State the role of `CsharpTestAutomation.Framework` and `CsharpTestAutomation.Tests`, then give the build command and the focused test command for a framework change.

Confirm the answer reflects the root instructions and the relevant scoped rule. For a task inside either project, confirm the nearest `AGENTS.md` and its canonical rule are used. A file being present is not proof that a host loaded it.

Run these read-only scenarios from a fresh root-started session in each supported
host. Also check the project scenarios from their project working directories.
Ask the agent to list files it actually loaded/read and the checks it would use.

| Scenario | Required guidance beyond the root | Guidance not required by this scenario |
| --- | --- | --- |
| Explain a framework utility change | Framework entry point, C# and framework rules; framework build/focused self-test commands. | Application API/UI rules and commit workflow. |
| Plan a change to an application API fixture | Application entry point, C#, shared test and API rules; API architecture when layer design changes. | UI rules unless UI behavior is involved. |
| Plan a page-object locator change | Application entry point, C#, shared test and UI rules; tooling/Playwright skill if live locator checks are available. | API rules unless API setup/verification is involved. |
| Plan a cross-layer UI/API scenario | Application entry point, C#, shared test, API and UI rules; relevant feature specification. | Unrelated feature specifications. |
| Explain a framework self-test change | Self-test entry point and C# rules; deterministic local checks and plain `await`. | Application test rules and external services. |
| Review without fixes | Review workflow plus the reviewed scope's entry point and canonical rules; no file changes. | Commit workflow. |
| Draft a documentation commit message without a work item | Commit workflow; `docs(automation): ...`, at most 72 characters, no invented work item, no commit. | Application API/UI rules and .NET build. |
| Maintain instruction links | This guide and affected instruction files; size/link and whitespace checks. | Live API/UI test runs. |

Report host activation separately from static file/link checks. Source presence
or a valid glob is not proof of activation. Record unavailable clients as not
tested; do not claim their discovery checks passed.

### Validation record

Record the date, host surface and version, scenarios, evidence, and result after
each check. Repeat discovery and connection checks after changes to client
versions, instruction routing, skills, or MCP launch settings. A connection check
in an existing session does not prove activation in a fresh session.

| Date | Host and version | Check and evidence | Result |
| --- | --- | --- | --- |
| 2026-10-04 | Codex workspace session; version not exposed | Root `AGENTS.md` was supplied in the session; read `docs/AGENT_SUPPORT.md` and `.agents/skills/graphify/SKILL.md`. The Graphify skill was the only repository skill read/loaded for this check. Called Context7 `resolve_library_id` for NUnit, Microsoft Learn `microsoft_docs_search` for `dotnet build`, Chrome DevTools `list_pages`, and FFF `find_files` for `AGENTS`. | All four MCP calls succeeded. Chrome listed `about:blank`; FFF found `AGENTS.md`. Fresh client restart/session check not tested; this session cannot restart the client. No files were edited during the discovery calls. |
| 2026-10-04 | Codex workspace session | Inspected the configured npm cache behavior in this guide. | `--prefer-offline` can use cached metadata; a cache miss still requires npm network access. Keep the current sandbox settings. Cache setup and a fresh-session check remain the next steps. |
| 2026-10-03 | Codex IDE session; CLI 0.160.0 | Root guidance and repository skill paths were supplied in this session; scoped guidance was read during review. | Partial; fresh-session scenario matrix not tested. |
| 2026-10-03 | Codex CLI 0.160.0 | TOML parsed with Python `tomllib`; `codex mcp list` and `codex mcp get` accepted all four server entries. | Passed; CLI reported sandbox access warnings for its temporary alias directory. |
| 2026-10-03 | Codex IDE session | FFF found `AGENTS.md`; Context7 resolved NUnit; Microsoft Learn found `dotnet build`; Chrome listed pages. | Passed in the existing session; does not validate changed launch settings. |
| 2026-10-03 | FFF 0.11.0 | Updated PowerShell launcher completed MCP initialization and listed three tools. | Passed. |
| 2026-10-03 | Chrome DevTools; local cache 1.10.1 | Separate npm startup timed out after 60 seconds; offline resolution reported `ENOTCACHED`, and the cache-preferred version check reported `EACCES` fetching npm metadata. | Blocked by sandbox network/cache access; repeat the configured startup check with npm access. |
| 2026-10-03 | GitHub Copilot | Fresh-session scenario matrix. | Not tested; no Copilot session available. |
| 2026-10-03 | Google Antigravity | Fresh-session scenario matrix. | Not tested; no Antigravity session available. |

## Instruction maintenance check

Run this PowerShell block from the repository root. It performs read-only checks
on the root/project entry points, canonical rules, workflow instructions,
Copilot adapters, and linked support/architecture documents. It validates local
Markdown file targets relative to their source files; URL targets are skipped.
Fragment existence and conditional routing require the discovery checks above.

```powershell
$instructionRoot = (Resolve-Path -LiteralPath '.').Path
$instructionRootFile = Join-Path $instructionRoot 'AGENTS.md'
$instructionLineCount = [System.IO.File]::ReadAllLines($instructionRootFile).Count
$instructionBytes = (Get-Item -LiteralPath $instructionRootFile).Length
Write-Output "Root AGENTS.md: $instructionLineCount lines; $instructionBytes bytes"
if ($instructionLineCount -gt 100) {
    throw "Root AGENTS.md exceeds the 100-line maximum: $instructionLineCount"
}
if ($instructionBytes -gt 5KB) {
    Write-Warning 'Root AGENTS.md exceeds the recommended 5 KiB context budget.'
}

$instructionFiles = @(
    Get-Item -LiteralPath 'AGENTS.md',
        'CsharpTestAutomation.Framework/AGENTS.md',
        'CsharpTestAutomation.Tests/AGENTS.md',
        'CsharpTestAutomation.Framework.Test/AGENTS.md',
        'README.md', 'docs/AGENT_SUPPORT.md',
        'docs/API_TESTING_ARCHITECTURE.md', 'docs/UI_TESTING_ARCHITECTURE.md',
        'docs/netbox-automation/03-device-management-spec.md'
    Get-ChildItem -LiteralPath '.agents/rules', '.agents/instructions',
        '.github/instructions' -Filter '*.md' -File
)
$instructionMissing = [System.Collections.Generic.List[string]]::new()
$instructionLinksChecked = 0
foreach ($instructionFile in $instructionFiles) {
    $instructionText = Get-Content -Encoding UTF8 -Raw -LiteralPath $instructionFile.FullName
    foreach ($instructionMatch in [regex]::Matches($instructionText, '\[[^\]\r\n]+\]\(([^)\r\n]+)\)')) {
        $instructionTarget = $instructionMatch.Groups[1].Value.Trim().Trim([char[]]'<>')
        if ($instructionTarget -match '^(?:[a-zA-Z][a-zA-Z0-9+.-]*:|#)') { continue }
        $instructionTarget = ($instructionTarget -split '#', 2)[0]
        if (-not $instructionTarget) { continue }
        $instructionTarget = [Uri]::UnescapeDataString($instructionTarget)
        $instructionResolved = Join-Path $instructionFile.DirectoryName $instructionTarget
        $instructionLinksChecked++
        if (-not (Test-Path -LiteralPath $instructionResolved -PathType Leaf)) {
            $instructionMissing.Add("$($instructionFile.FullName) -> $instructionTarget")
        }
    }
}
if ($instructionMissing.Count -gt 0) {
    throw ("Broken local links:`n" + ($instructionMissing -join "`n"))
}
Write-Output "Checked $instructionLinksChecked local links in $($instructionFiles.Count) files."
git diff --check
if ($LASTEXITCODE -ne 0) { throw 'git diff --check failed.' }
```

Instruction-only changes do not need a .NET build or live test execution.
Review the diff for preserved requirements, valid exceptions, and unrelated
working-tree edits before completion.

## Official References

- [VS Code custom instructions](https://code.visualstudio.com/docs/agent-customization/custom-instructions)
- [AGENTS.md format](https://agents.md/)
- [Codex AGENTS.md](https://learn.chatgpt.com/docs/agent-configuration/agents-md)
- [Codex skills](https://learn.chatgpt.com/docs/build-skills)
- [Codex MCP configuration](https://learn.chatgpt.com/docs/extend/mcp?surface=cli)
- [Codex configuration and project trust](https://learn.chatgpt.com/docs/config-file/config-basic)
- [Chrome DevTools MCP client configuration](https://github.com/ChromeDevTools/chrome-devtools-mcp/blob/main/docs/client-configurations.md)
- [npm cache preference](https://docs.npmjs.com/cli/v11/using-npm/config#prefer-offline)
- [Antigravity rules](https://antigravity.google/docs/rules/) and [skills](https://antigravity.google/docs/skills/)
- [Agent Skills specification](https://agentskills.io/specification)
