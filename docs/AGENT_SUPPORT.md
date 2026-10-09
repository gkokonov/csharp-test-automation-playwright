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
| NUnit application scope | `CsharpTestAutomation.Tests/AGENTS.md`; NUnit fixture bases, lifecycle, and metadata. |
| BDD application scope | `CsharpTestAutomation.Bdd.Tests/AGENTS.md` and `.agents/rules/bdd-testing.md`; Reqnroll features, bindings, hooks, typed state, and lifecycle. |
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

Choose the application project from the requested approach. If BDD versus plain
NUnit is unspecified and affects implementation, ask which is required. Do not
generate both. Shared API/UI rules retain assertion, readiness, data ownership,
and cleanup contracts; their fixture-base requirements are scoped to NUnit.
BDD hook equivalents are owned by the canonical BDD rule. All hosts use the
same project entry points; Copilot adapters remain path-specific pointers.

The [guidance reading checklist](../.agents/instructions/tooling.md#guidance-reading-checklist)
is owned by the tooling instructions and routed from root `AGENTS.md` for all
tasks. Keep one shared checklist; do not copy it into host adapters or personal
skills. A size/link check verifies the documentation, not agent compliance.
Use a fresh-session task to check that the agent follows the checklist.

API and UI globs include their client/page, fixture, base-fixture, and step
paths. Keep Copilot adapter `applyTo` patterns and Antigravity canonical `globs`
consistent. Do not add a catch-all rule that loads both layer documents.

When moving a rule, update its incoming links and read trigger in the same
change. Keep shared ownership/cleanup policy in the test rule; device-specific
identifiers, prerequisites, and delete outcomes belong in the
[Device specification](netbox-automation/03-device-management-spec.md).
Maintain the short root Codebase Memory MCP route; generated installer blocks must also
pass the root size check.

## Host Limits and Activation

- Codex combines its active instruction chain up to `project_doc_max_bytes`, which defaults to 32 KiB. Keep root and nested instructions concise.
- Antigravity scans immediate `.agents/rules/*.md` files. Each needs YAML frontmatter with a valid `trigger`; glob rules need `globs`. Nested rule directories need explicit registration.
- Agent Skills use the shared `SKILL.md` format. Keep each skill focused and describe both its purpose and when it should activate.
- Copilot's `.github/instructions/*.instructions.md` files are available for additional path-specific activation. Use them only for behavior that cannot be expressed or discovered through `AGENTS.md`.

## Shared software design skill

The [software design skill](../.agents/skills/software-design-principles/SKILL.md)
uses shared Markdown instructions and relative references. Codex and Copilot
document `.agents/skills/` discovery. Its `agents/openai.yaml` is optional Codex
metadata; it is not the owner of the design rules.

Claude Code documents `.claude/skills/` and supported symlinked skill folders.
This is a distribution option, not a configured or validated Claude adapter in
this checkout. Keep one canonical skill folder. See the skill's
[authoring and portability reference](../.agents/skills/software-design-principles/references/source-coverage-and-validation.md#authoring-and-portability)
for host documentation, checker prerequisites, and evaluation scope.

Validate discovery and resource reads in each host independently. The sixteen
canonical behavior cases use a semantic rubric; the six Waza tasks are a smoke
subset. Record unavailable models, exhausted quotas, and blocked file reads
separately from findings about the skill. Supplied-context evaluations can
check advice, but do not prove automatic discovery or reference loading.

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
The [tooling instructions](../.agents/instructions/tooling.md#codebase-memory-mcp)
include Codebase Memory MCP query guidance and a Windows CLI fallback.
The software-design skill retains its existing `agents/openai.yaml` metadata.

MCP prerequisites are:

| Server | Local requirement | Codex configuration |
| --- | --- | --- |
| Context7 | `CONTEXT7_API_KEY` in the environment inherited by the Codex host; network access | HTTP with an environment-supplied bearer token |
| Microsoft Learn | Network access | HTTP; retains `maxTokenBudget=5000` |
| Chrome DevTools | Node.js/npm with `npx` on `PATH`; Chrome available; npm access on first launch | `cmd.exe /d /c` launches the headless MCP command; Windows path variables are forwarded |
| FFF | `fff-mcp.exe` installed under `LOCALAPPDATA/fff-mcp/bin` | PowerShell resolves the path from the forwarded `LOCALAPPDATA` variable |
| Codebase Memory | Native Windows executable under `LOCALAPPDATA/Programs/codebase-memory-mcp` | Optional stdio entry with this checkout as its working directory |

Codebase Memory MCP uses its native Windows executable. Browser tasks need the
installed Playwright CLI or the local `npx --no-install playwright cli` command.
These tools are for agent development work; they do not change test dependencies.

Set credentials through the local host environment before starting Codex. If
an environment variable changes, restart the desktop app or IDE host so its
child processes receive the new value. Never store key values in the repository.

### Configuration boundaries and fallbacks

`.codex/config.toml` defines five optional MCP servers for local Windows
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

If FFF or Codebase Memory MCP is unavailable, use targeted `rg` searches and file reads.
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
codex mcp get codebase-memory-mcp
```

These commands inspect configuration; they do not prove a server connected.
In a fresh client session, use `/mcp` where supported to inspect active server
status. Request one small tool call from each available server: resolve the
NUnit library with Context7, search Microsoft Learn for `dotnet build`, list
Chrome pages, and find `AGENTS.md` with FFF. Report connection failures separately.

Use the discovery prompt below at the root and in all three project
directories. Confirm the response uses actual loaded/read files, identifies
the repository skills, and applies the correct canonical rules. Also
check a root-started task that targets a project; it must explicitly read the
project instructions. Instruction files alone do not prove activation.

To check conditional routing, request a commit message for a documentation
change without a work item. Expect `docs(automation): ...`, at most 72
characters, no invented work item, and no commit. Request a review without
fixes, then compare `git diff` before and after; the review must not modify
files. Configuration and instruction-only changes do not need a .NET build.

## Codebase Memory MCP on Windows

Use Codebase Memory MCP for code navigation and FFF for file discovery. CBM
provides symbol search, schema queries, and call tracing through the shared
[tooling instructions](../.agents/instructions/tooling.md#codebase-memory-mcp).
Existing `.gitignore` rules apply; no `.cbmignore` is required.

### Setup validation: 2026-10-06

| Check | Evidence and result |
| --- | --- |
| Installation | Official Windows installer verified the release SHA-256 checksum and installed version 0.11.0. |
| Runtime settings | `config list` confirmed the four applied indexing/watcher values. `watch_non_git=false` failed with unknown config key; omitted for this release. |
| Codex configuration | `codex mcp get codebase-memory-mcp` accepted the stdio entry, executable, checkout, environment variables, and 60-second startup timeout. |
| Explicit index | `cli index_repository` completed: 4,409 nodes, 8,399 edges, zero partial/unusable parses. Local `appsettings.local.json` was excluded by `.gitignore`. |
| CLI queries | `list_projects`, `get_graph_schema`, and `search_graph` succeeded. Search found `CoreConfiguration` at `CsharpTestAutomation.Framework/Common/CoreConfiguration.cs`, lines 14–50. |
| Direct MCP protocol | Local stdio initialization, `tools/list`, and `list_projects` passed; server identified version 0.11.0 and exposed 17 tools. This was a separate process, not a Codex/Copilot/Antigravity activation check. |
| Static checks | TOML and both JSON adapters parsed; executable and checkout paths verified. Root `AGENTS.md`: 79 lines, 4,593 bytes. Checked 75 local Markdown links; `git diff --check` passed. |
| Fresh host activation | Codex, VS Code Copilot, and Antigravity restarts and tool calls remain pending. Local CLI checks do not prove host activation. |
| Watcher refresh | Enabled in runtime settings; refresh after a saved C# change remains pending in a connected client. |

### Navigation migration: 2026-10-06

Codebase Memory MCP is now the repository code navigation tool. Root
`AGENTS.md` and the tooling instructions own its query, coverage, refresh, and
CLI fallback guidance. No CBM skill is required. The retired navigation skill
and its references were removed; optional `.codebase-memory/` exports are ignored.

In this Codex session, direct MCP calls to `list_projects`, `get_graph_schema`,
and `search_graph` succeeded. Search found `CoreConfiguration` in the expected
framework source file. Copilot/Antigravity activation and watcher refresh after
a saved C# change remain untested. Repository reference search found no remaining
retired tool names or CBM skill links. Root `AGENTS.md` remains 79 lines; the
maintenance check passed 75 local links and `git diff --check`.

## Discovery Check

For each supported host and surface, start a fresh session at the repository root and ask:

> List the repository instruction files and skills you loaded or actually read. State the role of the framework, NUnit application, and BDD application projects, then give the build and focused test commands for a framework change.

Confirm the answer reflects the root instructions and the relevant scoped rule. For a task inside each project, confirm the nearest `AGENTS.md` and its canonical rules are used. A file being present is not proof that a host loaded it.

Run these read-only scenarios from a fresh root-started session in each supported
host. Also check the project scenarios from their project working directories.
Ask the agent to list files it actually loaded/read and the checks it would use.

| Scenario | Required guidance beyond the root | Guidance not required by this scenario |
| --- | --- | --- |
| Explain a framework utility change | Framework entry point, C# and framework rules; framework build/focused self-test commands. | Application API/UI rules and commit workflow. |
| Plan a change to an application API fixture | Application entry point, C#, shared test and API rules; API architecture when layer design changes. | UI rules unless UI behavior is involved. |
| Plan a page-object locator change | Application entry point, C#, shared test and UI rules; tooling/Playwright skill if live locator checks are available. | API rules unless API setup/verification is involved. |
| Plan a cross-layer UI/API scenario | Application entry point, C#, shared test, API and UI rules; relevant feature specification. | Unrelated feature specifications. |
| Plan a BDD Site API scenario | BDD entry point, C#, shared test, BDD and API rules; explicit BDD project and stable-ID filter. | UI rules and NUnit fixture bases. |
| Plan a BDD Site UI/API scenario | BDD entry point, C#, shared test, BDD, API and UI rules; hooks and typed scenario state. | NUnit fixture bases and Allure.NUnit activation. |
| Plan from inside the BDD project | Root and BDD entry point; relevant canonical rules loaded through links. | The NUnit application entry point unless comparison is requested. |
| Add an application scenario without specifying an approach | Root project-choice route; ask BDD or NUnit before dependent implementation. | Implicit generation of both approaches. |
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
| 2026-10-09 | Codex CLI 0.161.0 | Three fresh read-only ephemeral sessions: BDD UI/API from the root, NUnit API from its project directory, and BDD API from its project directory. Responses distinguish supplied instructions from read files, select the correct scope/adapter/commands, retain eight product cases, and ask BDD versus NUnit for an ambiguous root request. JSON responses, event logs, and `audit.json` are under `artifacts/bdd-validation/phase-5/`. | All three probes and semantic response checks passed. RTK read-command failures were recovered with source reads. No builds/tests or file edits by the probes. Desktop/IDE activation is not tested. |
| 2026-10-09 | GitHub Copilot and Google Antigravity | Fresh-session BDD/NUnit discovery matrix. No callable CLI was found on PATH, and no interactive session was available. | Not tested. Static adapter/glob validation does not prove host activation. |
| 2026-10-09 | Repository static checks | Extracted and executed the maintenance block below after Phase 5 edits. Root is 84 lines/4961 bytes; 128 local links across 30 files; heading fragments; 18 representative routes; four canonical/adapter glob pairs; whitespace checks. | Passed. Separate read-only GPT-6 Luna review at high effort found no material issues. These checks do not validate untested host surfaces. |

## Instruction maintenance check

The Phase 5 discovery records above precede the BDD step consolidation. The
maintenance routes below use the current `Steps/` paths. Fresh host activation
after that follow-up must be distinguished from the earlier discovery evidence.

Run this PowerShell block from the repository root. It performs read-only checks
on the root/project entry points, canonical rules, workflow instructions,
Copilot adapters, and linked support/architecture documents. It validates local
Markdown file targets and heading fragments relative to their source files; URL
targets are skipped. It checks canonical/adapter glob equality and representative
path routing. Host activation still requires the fresh-session checks above.

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
        'CsharpTestAutomation.Bdd.Tests/AGENTS.md',
        'CsharpTestAutomation.Framework.Test/AGENTS.md',
        'README.md', 'docs/AGENT_SUPPORT.md',
        'docs/API_TESTING_ARCHITECTURE.md', 'docs/UI_TESTING_ARCHITECTURE.md',
        'docs/netbox-automation/03-device-management-spec.md'
    Get-ChildItem -LiteralPath '.agents/rules', '.agents/instructions',
        '.github/instructions' -Filter '*.md' -File
)
$instructionMissing = [System.Collections.Generic.List[string]]::new()
$instructionLinksChecked = 0
function Get-InstructionAnchors([string] $path) {
    $anchors = [System.Collections.Generic.HashSet[string]]::new()
    $duplicates = @{}
    $fenced = $false
    foreach ($line in [System.IO.File]::ReadAllLines($path)) {
        if ($line -match '^\s*(```|~~~)') { $fenced = -not $fenced; continue }
        if ($fenced -or $line -notmatch '^#{1,6}\s+(.+?)\s*#*\s*$') { continue }
        $slug = [regex]::Replace($Matches[1].ToLowerInvariant(), '[^\p{L}\p{N}_\- ]', '').Replace(' ', '-')
        $anchor = $slug
        if ($duplicates.ContainsKey($slug)) { $duplicates[$slug]++; $anchor = "$slug-$($duplicates[$slug])" }
        else { $duplicates[$slug] = 0 }
        [void]$anchors.Add($anchor)
    }
    return ,$anchors
}
foreach ($instructionFile in $instructionFiles) {
    $instructionText = Get-Content -Encoding UTF8 -Raw -LiteralPath $instructionFile.FullName
    foreach ($instructionMatch in [regex]::Matches($instructionText, '\[[^\]\r\n]+\]\(([^)\r\n]+)\)')) {
        $instructionTarget = $instructionMatch.Groups[1].Value.Trim().Trim([char[]]'<>')
        if ($instructionTarget -match '^[a-zA-Z][a-zA-Z0-9+.-]*:') { continue }
        $instructionParts = $instructionTarget -split '#', 2
        $instructionFileTarget = [Uri]::UnescapeDataString($instructionParts[0])
        $instructionResolved = if ($instructionFileTarget) {
            Join-Path $instructionFile.DirectoryName $instructionFileTarget
        } else { $instructionFile.FullName }
        $instructionLinksChecked++
        if (-not (Test-Path -LiteralPath $instructionResolved -PathType Leaf)) {
            $instructionMissing.Add("$($instructionFile.FullName) -> $instructionTarget")
            continue
        }
        if ($instructionParts.Count -eq 2 -and $instructionParts[1]) {
            $instructionAnchors = Get-InstructionAnchors $instructionResolved
            if (-not $instructionAnchors.Contains([Uri]::UnescapeDataString($instructionParts[1]))) {
                $instructionMissing.Add("$($instructionFile.FullName) -> $instructionTarget (fragment)")
            }
        }
    }
}
if ($instructionMissing.Count -gt 0) {
    throw ("Broken local links:`n" + ($instructionMissing -join "`n"))
}
Write-Output "Checked $instructionLinksChecked local links in $($instructionFiles.Count) files."

$instructionGlobs = @{}
foreach ($instructionPair in @(
    @('test-automation', 'application-tests'), @('api-testing', 'api-testing'),
    @('ui-testing', 'ui-testing'), @('bdd-testing', 'bdd-testing')
)) {
    $instructionRule = Get-Content -Raw -LiteralPath ".agents/rules/$($instructionPair[0]).md"
    $instructionAdapter = Get-Content -Raw -LiteralPath ".github/instructions/$($instructionPair[1]).instructions.md"
    $instructionPatterns = @([regex]::Match($instructionRule, '(?m)^globs: "([^"]+)"').Groups[1].Value.Split(',').Trim())
    $instructionApply = @([regex]::Match($instructionRule, '(?m)^applyTo: "([^"]+)"').Groups[1].Value.Split(',').Trim())
    $instructionCopilot = @([regex]::Match($instructionAdapter, '(?m)^applyTo: "([^"]+)"').Groups[1].Value.Split(',').Trim())
    if (-not $instructionPatterns[0] -or
        (Compare-Object $instructionPatterns $instructionApply) -or
        (Compare-Object $instructionPatterns $instructionCopilot)) {
        throw "Canonical/adapter glob mismatch: $($instructionPair[0])"
    }
    $instructionGlobs[$instructionPair[0]] = $instructionPatterns
}
function Test-InstructionGlob([string] $path, [string[]] $patterns) {
    foreach ($pattern in $patterns) {
        $regex = '^' + [regex]::Escape($pattern).Replace('\*\*/', '(?:.*/)?').Replace('\*', '[^/]*') + '$'
        if ([regex]::IsMatch($path, $regex)) { return $true }
    }
    return $false
}
# Columns: path, shared application rules, API rules, UI rules, BDD rules.
$instructionCases = @(
    @('CsharpTestAutomation.Tests/API/Clients/SitesApiClient.cs', $true, $true, $false, $false),
    @('CsharpTestAutomation.Tests/Tests/ApiTestBase.cs', $true, $true, $false, $false),
    @('CsharpTestAutomation.Tests/Tests/API/SitesApiTests.cs', $true, $true, $false, $false),
    @('CsharpTestAutomation.Tests/UI/Pages/NetBox/SitesListPage.cs', $true, $false, $true, $false),
    @('CsharpTestAutomation.Tests/Tests/UiTestBase.cs', $true, $false, $true, $false),
    @('CsharpTestAutomation.Tests/Tests/UI/NetBox/SiteManagementUiTests.cs', $true, $false, $true, $false),
    @('CsharpTestAutomation.Bdd.Tests/API/Clients/SitesApiClient.cs', $true, $true, $false, $true),
    @('CsharpTestAutomation.Bdd.Tests/Steps/API/NetBox/SiteApiSteps.cs', $true, $true, $false, $true),
    @('CsharpTestAutomation.Bdd.Tests/UI/Pages/NetBox/SitesListPage.cs', $true, $false, $true, $true),
    @('CsharpTestAutomation.Bdd.Tests/Steps/UI/NetBox/SiteUiSteps.cs', $true, $false, $true, $true),
    @('CsharpTestAutomation.Bdd.Tests/Features/SiteApi.feature', $false, $true, $false, $true),
    @('CsharpTestAutomation.Bdd.Tests/Features/SiteUi.feature', $false, $false, $true, $true),
    @('CsharpTestAutomation.Bdd.Tests/Hooks/ScenarioHooks.cs', $true, $false, $false, $true),
    @('CsharpTestAutomation.Bdd.Tests/Context/SiteScenarioState.cs', $true, $false, $false, $true),
    @('CsharpTestAutomation.Bdd.Tests/AssemblyInfo.cs', $true, $false, $false, $true),
    @('CsharpTestAutomation.Bdd.Tests/reqnroll.json', $false, $false, $false, $true),
    @('CsharpTestAutomation.Framework/API/Clients/RestClientFactory.cs', $false, $false, $false, $false),
    @('CsharpTestAutomation.Framework.Test/AssemblyInfo.cs', $false, $false, $false, $false)
)
foreach ($instructionCase in $instructionCases) {
    $instructionIndex = 1
    foreach ($instructionRuleName in @('test-automation', 'api-testing', 'ui-testing', 'bdd-testing')) {
        if ((Test-InstructionGlob $instructionCase[0] $instructionGlobs[$instructionRuleName]) -ne $instructionCase[$instructionIndex]) {
            throw "Unexpected routing: $($instructionCase[0]) -> $instructionRuleName"
        }
        $instructionIndex++
    }
}
Write-Output "Checked $($instructionCases.Count) path routes and four canonical/adapter glob pairs."
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
