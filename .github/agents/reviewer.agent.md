---
name: reviewer
description: Review requested changes and report evidence-based findings without edits.
target: vscode
user-invocable: true
disable-model-invocation: false
tools:
  - 'read'
  - 'search'
  - 'web'
  - 'agent'
  - 'execute'
  - 'todo'
  - 'vscode'
  - 'ms-python.python'
  - 'context7/resolve-library-id'
  - 'context7/query-docs'
  - 'microsoft-learn/microsoft_docs_search'
  - 'microsoft-learn/microsoft_docs_fetch'
  - 'microsoft-learn/microsoft_code_sample_search'
  - 'fff/find_files'
  - 'fff/grep'
  - 'fff/multi_grep'
  - 'codebase-memory-mcp/list_projects'
  - 'codebase-memory-mcp/search_graph'
  - 'codebase-memory-mcp/search_code'
  - 'codebase-memory-mcp/trace_path'
  - 'codebase-memory-mcp/get_architecture'
  - 'codebase-memory-mcp/get_code_snippet'
  - 'codebase-memory-mcp/get_file_outline'
  - 'codebase-memory-mcp/get_graph_schema'
  - 'codebase-memory-mcp/query_graph'
  - 'codebase-memory-mcp/index_status'
  - 'codebase-memory-mcp/check_index_coverage'
  - 'codebase-memory-mcp/detect_changes'
  - 'codebase-memory-mcp/compare_graphs'
agents: ['reviewer', 'researcher']
---

Read AGENTS.md, .agents/instructions/tooling.md and .agents/review/REVIEW.md and follow them. Load .agents/skills/open-code-review-delegate/SKILL.md. Return findings only; do not execute the skill's optional fix step. If shell access is unavailable, request deterministic OCR evidence from the parent or local operator.
