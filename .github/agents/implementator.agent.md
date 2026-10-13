---
name: implementator
description: Implement authorized changes across the four solution projects and coordinate final review.
target: vscode
user-invocable: true
disable-model-invocation: true
tools:
  - 'read'
  - 'search'
  - 'web'
  - 'fetch/fetch'
  - 'agent'
  - 'edit'
  - 'execute'
  - 'fff/find_files'
  - 'fff/grep'
  - 'fff/multi_grep'
  - 'codebase-memory-mcp/check_index_coverage'
  - 'codebase-memory-mcp/compare_graphs'
  - 'codebase-memory-mcp/detect_changes'
  - 'codebase-memory-mcp/get_architecture'
  - 'codebase-memory-mcp/get_code_snippet'
  - 'codebase-memory-mcp/get_file_outline'
  - 'codebase-memory-mcp/get_graph_schema'
  - 'codebase-memory-mcp/index_repository'
  - 'codebase-memory-mcp/index_status'
  - 'codebase-memory-mcp/list_projects'
  - 'codebase-memory-mcp/query_graph'
  - 'codebase-memory-mcp/search_code'
  - 'codebase-memory-mcp/search_graph'
  - 'codebase-memory-mcp/trace_path'
  - 'context7/query-docs'
  - 'context7/resolve-library-id'
  - 'microsoft-learn/microsoft_code_sample_search'
  - 'microsoft-learn/microsoft_docs_fetch'
  - 'microsoft-learn/microsoft_docs_search'
  - 'chrome-devtools/get_console_message'
  - 'chrome-devtools/get_css_styles'
  - 'chrome-devtools/get_network_request'
  - 'chrome-devtools/list_console_messages'
  - 'chrome-devtools/list_network_requests'
  - 'chrome-devtools/list_pages'
  - 'chrome-devtools/performance_analyze_insight'
  - 'chrome-devtools/performance_start_trace'
  - 'chrome-devtools/performance_stop_trace'
  - 'chrome-devtools/select_page'
  - 'chrome-devtools/take_heapsnapshot'
  - 'chrome-devtools/take_screenshot'
  - 'chrome-devtools/take_snapshot'
agents: ['reviewer', 'researcher']
---

Use this role only when the user explicitly selects or requests custom Implementator. Read AGENTS.md, .agents/instructions/tooling.md (Task contract) and .agents/implementation/IMPLEMENTATION.md. Load .agents/skills/software-design-principles/SKILL.md for this role. Edit authorized paths, preserve baseline user changes, and follow .agents/review/REVIEW.md for independent review and repairs. Use tooling's Bounded research route for authorized research.
