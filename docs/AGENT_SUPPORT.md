# Coding Agent Support

This repository uses shared `AGENTS.md` instructions and Agent Skills. Avoid provider-specific copies of the same rules. Keep host adapters small and add them only when discovery tests show that a host needs them.

## Discovery Matrix

| Host | Shared repository instructions | Scoped guidance | Skills |
| --- | --- | --- | --- |
| GitHub Copilot (IDE and cloud) | Root `AGENTS.md` | `.github/instructions/*.instructions.md` path pointers and project `AGENTS.md` | `.agents/skills/` |
| OpenAI Codex (desktop, IDE, and CLI) | Root-to-working-directory `AGENTS.md` chain | Project `AGENTS.md`; read linked canonical rules | `.agents/skills/` |
| Google Antigravity (IDE and CLI) | Root and directory-scoped `AGENTS.md` | `.agents/rules/*.md` also loads by matching `trigger`/`globs` | `.agents/skills/` |
| Devin (cloud and CLI) | Root and directory-scoped `AGENTS.md` | Project `AGENTS.md`; the scoped files point to canonical rules | `.agents/skills/` |

The scoped files under `CsharpTestAutomation.Framework/`, `CsharpTestAutomation.Framework.Test/`, and `CsharpTestAutomation.Tests/` are short discovery entry points. The `.github/instructions/` files provide path-based Copilot discovery and point to the same canonical rules. The detailed rules remain in `.agents/rules/` for Antigravity and for hosts directed to read them. Do not copy their contents into provider-specific files.

## Host Limits and Activation

- Codex combines its active instruction chain up to `project_doc_max_bytes`, which defaults to 32 KiB. Keep root and nested instructions concise.
- Devin automatically injects up to 16 KiB from each `AGENTS.md`. Put critical project instructions near the top and move workflows to skills.
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
| Chrome DevTools | Node.js/npm with `npx` on `PATH`; Chrome available; npm access on first launch | `cmd.exe` launches the existing headless MCP command |
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

## Official References

- [VS Code custom instructions](https://code.visualstudio.com/docs/agent-customization/custom-instructions)
- [AGENTS.md format](https://agents.md/)
- [Codex AGENTS.md](https://learn.chatgpt.com/docs/agent-configuration/agents-md)
- [Codex skills](https://learn.chatgpt.com/docs/build-skills)
- [Codex MCP configuration](https://learn.chatgpt.com/docs/extend/mcp?surface=cli)
- [Codex configuration and project trust](https://learn.chatgpt.com/docs/config-file/config-basic)
- [Antigravity rules](https://antigravity.google/docs/rules/) and [skills](https://antigravity.google/docs/skills/)
- [Devin AGENTS.md](https://docs.devin.ai/onboard-devin/agents-md) and [skills](https://docs.devin.ai/product-guides/skills)
- [Agent Skills specification](https://agentskills.io/specification)
