# Coding Agent Support

This guide covers host setup and validation. Repository rules belong in the
root [AGENTS.md](../AGENTS.md), scoped rules, and shared
[tooling instructions](../.agents/instructions/tooling.md). Keep host adapters
as pointers; do not duplicate canonical policy.

## Host discovery

| Host | Instruction and skill sources | Host adapter |
| --- | --- | --- |
| GitHub Copilot in VS Code | Root and project `AGENTS.md`; `.agents/skills/` | `.github/instructions/` points to canonical rules; `.github/agents/` defines local agents. |
| OpenAI Codex | Root-to-working-directory `AGENTS.md`; `.agents/skills/` | `.codex/agents/`; `.codex/config.toml` holds optional Codex MCP settings. |
| Google Antigravity | Root and scoped `AGENTS.md`; matching `.agents/rules/`; `.agents/skills/` | `.agents/agents/`; `.agents/mcp_config.json` holds its MCP settings. |

Use a fresh session to check host discovery after changing instruction routing,
skills, adapters, or MCP launch settings. File presence and static parsing do not
prove that a host loaded a file or enforced its permissions.

Keep Antigravity rules in the immediate `.agents/rules/` directory with valid
`trigger` metadata and `globs` where needed. Copilot `applyTo` patterns must
match the canonical globs. Codex follows explicit reading routes in `AGENTS.md`;
it does not use Antigravity frontmatter for routing. Keep one canonical skill
under `.agents/skills/` and validate its discovery and linked resources in each
host. See the skill's [portability notes](../.agents/skills/software-design-principles/references/source-coverage-and-validation.md#authoring-and-portability)
for the shared software-design skill.

## Local implementation, review, and research agents

Select `implementator`, `reviewer`, or `researcher` from the host's local agent
picker. Codex requires a trusted project and fresh session. These adapters use
the shared [implementation](../.agents/implementation/IMPLEMENTATION.md) and
[review](../.agents/review/REVIEW.md) workflows. Researcher is for bounded
research and cannot implement changes.

Use [tooling's role access rules](../.agents/instructions/tooling.md#agent-tool-access)
for the selected navigation/documentation tools, external service caches, and
Implementator browser diagnostics. Codex/Copilot contain explicit MCP allowlists.
Antigravity grants documented built-in search and URL-fetch tools; custom-agent
MCP binding/filtering still needs fresh host verification. Follow the
[RTK evidence rules](../.agents/instructions/tooling.md#rtk-and-complete-evidence)
when preparing complete patches, source, or OCR JSON.

Reviewer must preserve no-edit behavior. Read-only isolation is the default;
an explicit user-approved `workspace-write` reviewer is permitted under the
[review workflow](../.agents/review/REVIEW.md). The repository's Codex reviewer
uses that approved mode. Filesystem writes are possible in this mode; report
that limit and verify effective permissions, including MCP tools, before
delegation. Adapter defaults do not prove runtime isolation. If the required
sandbox cannot be verified, use a separate local session or immutable snapshot.
Researcher remains read-only. Reviewer helpers follow the shared workflow and
cannot delegate further. Do not use destructive write probes to test permissions.

OCR can omit task-owned host settings or excluded provider files. Check the
manifest exclusions and review task-owned settings separately. Report that
coverage apart from OCR-selected files; do not call an incomplete selection a
complete review.

## Codex on Windows

Open this checkout as a trusted project and start a fresh session. Codex reads
the root-to-working-directory `AGENTS.md` chain and repository skills. Keep
credentials in the local host environment, never in repository files; restart
the host after changing environment variables. `.codex/config.toml` is optional
MCP configuration and does not set the model, sandbox, or approval policy.

Inspect configured servers with `codex mcp list` and `codex mcp get <name>`.
Then check `/mcp` in a fresh session and make a small call to each available
server. Configuration output alone does not prove a connection. Use the
[tooling instructions](../.agents/instructions/tooling.md) for server-specific
requirements and fallbacks. `.vscode/mcp.json` and
`.agents/mcp_config.json` are separate host configurations; check each affected
adapter when changing a shared endpoint or launch option.

### Review fallback when shell reads are rejected

A separate local CLI review can still reject harmless shell reads after a
`--sandbox workspace-write` request when the effective session reports
`read-only`. Keep the approved Reviewer TOML setting; the CLI request and static
adapter checks do not prove native host activation or effective tool access.
Use the review workflow's supplied-evidence route:

1. The Implementator runs deterministic OCR preview/rule commands and prepares
   complete task patches, source snapshots, applicable instructions, API/UI
   architecture references, RTK guidance, validation results, and content hashes.
2. Start a separate local Reviewer with that fixed evidence and instruct it to
   review without tool calls. Check the effective session sandbox, absence of
   configured MCP services, and the session log for tool calls. A no-call review
   is an observation, not proof that its tool schema lacks tools. The Reviewer
   reports findings and reviewed/skipped identities from the supplied snapshot;
   it does not claim to have run OCR, builds, tests, or repository reads.
3. Keep edits paused. Compare current task hashes with the snapshot before
   accepting the report. Supply missing references or changed content and repeat
   the whole task review. Report native host activation as unverified.

This fallback establishes an independent static review of supplied content.
For example, after preparing the fixed prompt and sources:

```powershell
Get-Content -Raw artifacts/agent-follow-up/snapshot-review-prompt.txt |
    codex exec --ignore-user-config --sandbox workspace-write --ephemeral --color never `
    -c 'features.multi_agent=false' -c 'web_search="disabled"' `
    --output-last-message artifacts/agent-follow-up/snapshot-review-result.txt - `
    > artifacts/agent-follow-up/snapshot-review-session.log 2>&1
```

The prompt forbids tool calls and includes OCR preview/rules, the full patch,
numbered source and guidance, and a content-hash manifest. Inspect the session
log and recheck hashes; the command alone does not establish complete coverage.
Live test evidence remains the Implementator's responsibility. For CBM timestamp
warnings and unsupported Gherkin sources, use the
[Windows freshness audit](../.agents/instructions/tooling.md#windows-freshness-and-unsupported-sources)
and direct `.feature` reads plus Reqnroll discovery.

## Discovery check

In a fresh session, ask:

> List the instruction and skill files you actually loaded or read. Explain the
> four project responsibilities and give the build and focused test commands
> for a framework change.

Repeat from the repository root and each project directory. Check one routed
task per affected scope, including the BDD/NUnit choice when it is unspecified.
For local agents, check role discovery, the required workflow, effective tools,
and a complete review/fix cycle in an isolated fixture. Record unavailable
hosts as untested. A file list or tool connection check alone does not prove
activation or correct behavior.

## Instruction maintenance check

From the repository root, run the [maintenance script](../.agents/scripts/Test-AgentHarness.ps1)
and [configuration checker](../.agents/scripts/check_agent_configuration.py):

```powershell
pwsh -NoProfile -File .agents/scripts/Test-AgentHarness.ps1
python .agents/scripts/check_agent_configuration.py
```

Use `-OcrFixtures` on the maintenance script when changing OCR selection or
rules. It requires Git and a JSON-capable OCR CLI. These checks cover links,
routing, adapter syntax, and deterministic fixtures; they do not prove host
activation or effective runtime isolation. Instruction-only changes need no
.NET build. Finish with `git diff --check` and review the complete diff.
