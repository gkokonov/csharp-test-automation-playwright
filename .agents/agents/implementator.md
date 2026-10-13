---
name: implementator
description: Implement authorized changes across the four solution projects and coordinate final review.
mainAgent: true
subagent: false
commandExecutionPolicy: "auto"
model: inherit
tools:
  - view_file
  - find_by_name
  - grep_search
  - search_web
  - read_url_content
  - list_permissions
  - invoke_subagent
  - run_command
  - replace_file_content
---

# Implementator Details

Read AGENTS.md, .agents/instructions/tooling.md and .agents/implementation/IMPLEMENTATION.md and follow them. Load .agents/skills/software-design-principles/SKILL.md for every implementation. Custom agents use the effective unrestricted shell/edit environment; follow each role's source and task boundaries. Use reviewer for the independent final review and researcher only for substantial research.

Use the tooling rules for connected MCP services, including the shared Fetch and browser fallback route. This adapter grants documented built-in tools only; use read_url_content for URLs or request Fetch evidence from the parent/operator until exact role-scoped MCP bindings and effective permissions are verified in this host. Do not insert SDK tool aliases or MCP permission selectors into tools.
