# Coding Agent Support

This repository uses shared `AGENTS.md` instructions and Agent Skills. Avoid provider-specific copies of the same rules. Keep host adapters small and add them only when discovery tests show that a host needs them.

## Discovery Matrix

| Host | Shared repository instructions | Scoped guidance | Skills |
| --- | --- | --- | --- |
| GitHub Copilot (IDE and cloud) | Root `AGENTS.md` | `.github/instructions/*.instructions.md` path pointers and project `AGENTS.md` | `.agents/skills/` |
| OpenAI Codex (IDE and CLI) | Root-to-working-directory `AGENTS.md` chain | Project `AGENTS.md` | `.agents/skills/` |
| Google Antigravity (IDE and CLI) | Root and directory-scoped `AGENTS.md` | `.agents/rules/*.md` also loads by matching `trigger`/`globs` | `.agents/skills/` |
| Devin (cloud and CLI) | Root and directory-scoped `AGENTS.md` | Project `AGENTS.md`; the scoped files point to canonical rules | `.agents/skills/` |

The scoped files under `CsharpTestAutomation.Framework/`, `CsharpTestAutomation.Framework.Test/`, and `CsharpTestAutomation.Tests/` are short discovery entry points. The `.github/instructions/` files provide path-based Copilot discovery and point to the same canonical rules. The detailed rules remain in `.agents/rules/` for Antigravity and for hosts directed to read them. Do not copy their contents into provider-specific files.

## Host Limits and Activation

- Codex combines its active instruction chain up to `project_doc_max_bytes`, which defaults to 32 KiB. Keep root and nested instructions concise.
- Devin automatically injects up to 16 KiB from each `AGENTS.md`. Put critical project instructions near the top and move workflows to skills.
- Antigravity scans immediate `.agents/rules/*.md` files. Each needs YAML frontmatter with a valid `trigger`; glob rules need `globs`. Nested rule directories need explicit registration.
- Agent Skills use the shared `SKILL.md` format. Keep each skill focused and describe both its purpose and when it should activate.
- Copilot's `.github/instructions/*.instructions.md` files are available for additional path-specific activation. Use them only for behavior that cannot be expressed or discovered through `AGENTS.md`.

## Discovery Check

For each supported host and surface, start a fresh session at the repository root and ask:

> List the repository instruction files and skills you loaded. State the role of `CsharpTestAutomation.Framework` and `CsharpTestAutomation.Tests`, then give the build command and the focused test command for a framework change.

Confirm the answer reflects the root instructions and the relevant scoped rule. For a task inside either project, confirm the nearest `AGENTS.md` and its canonical rule are used. A file being present is not proof that a host loaded it.

## Official References

- [VS Code custom instructions](https://code.visualstudio.com/docs/agent-customization/custom-instructions)
- [AGENTS.md format](https://agents.md/)
- [Codex AGENTS.md](https://learn.chatgpt.com/docs/agent-configuration/agents-md)
- [Antigravity rules](https://antigravity.google/docs/rules/) and [skills](https://antigravity.google/docs/skills/)
- [Devin AGENTS.md](https://docs.devin.ai/onboard-devin/agents-md) and [skills](https://docs.devin.ai/product-guides/skills)
- [Agent Skills specification](https://agentskills.io/specification)
