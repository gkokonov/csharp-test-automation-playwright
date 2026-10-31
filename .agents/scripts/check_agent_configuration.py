"""Validate adapter syntax, instruction references, agent tools, and OCR routes."""

import argparse
import contextlib
import fnmatch
import json
import io
from pathlib import Path
import re
import sys
import tomllib
import shutil

from agent_harness_common import disposable_repository, require


# Validate the role policy owned by .agents/instructions/tooling.md.
MCP_INSPECTION = {
    "fetch": {"fetch"},
    "fff": {"find_files", "grep", "multi_grep"},
    "codebase-memory-mcp": {
        "list_projects", "search_graph", "search_code", "trace_path", "get_architecture",
        "get_code_snippet", "get_file_outline", "get_graph_schema", "query_graph",
        "index_status", "check_index_coverage", "detect_changes", "compare_graphs",
    },
    "context7": {"resolve-library-id", "query-docs"},
    "microsoft-learn": {
        "microsoft_docs_search", "microsoft_docs_fetch", "microsoft_code_sample_search",
    },
}
CHROME_DIAGNOSTICS = {
    "list_pages", "select_page", "list_console_messages", "get_console_message",
    "list_network_requests", "get_network_request", "take_snapshot", "take_screenshot",
    "get_css_styles", "performance_start_trace", "performance_stop_trace",
    "performance_analyze_insight", "take_heapsnapshot",
}
FIRECRAWL_TOOLS = {
    "firecrawl_agent",
    "firecrawl_agent_status",
    "firecrawl_check_crawl_status",
    "firecrawl_crawl",
    "firecrawl_credit_usage",
    "firecrawl_developer_search",
    "firecrawl_feedback",
    "firecrawl_find_tools",
    "firecrawl_gov_search",
    "firecrawl_interact",
    "firecrawl_interact_stop",
    "firecrawl_map",
    "firecrawl_monitor_check",
    "firecrawl_monitor_checks",
    "firecrawl_monitor_create",
    "firecrawl_monitor_delete",
    "firecrawl_monitor_get",
    "firecrawl_monitor_list",
    "firecrawl_monitor_run",
    "firecrawl_monitor_update",
    "firecrawl_parse",
    "firecrawl_research_inspect_paper",
    "firecrawl_research_read_paper",
    "firecrawl_research_related_papers",
    "firecrawl_research_search_papers",
    "firecrawl_scrape",
    "firecrawl_search",
    "firecrawl_search_feedback",
}


def role_mcp_tools(role):
    tools = {server: set(names) for server, names in MCP_INSPECTION.items()}
    if role == "implementator":
        tools["codebase-memory-mcp"].add("index_repository")
        tools["chrome-devtools"] = set(CHROME_DIAGNOSTICS)
        tools["firecrawl"] = set(FIRECRAWL_TOOLS)
    elif role == "researcher":
        tools["firecrawl"] = set(FIRECRAWL_TOOLS)
    return tools


def check_mcp(root, path, data, role, project, vscode):
    servers = data.get("mcp_servers", {})
    expected = role_mcp_tools(role)
    require(set(servers) == set(MCP_INSPECTION) | {"chrome-devtools", "firecrawl"},
            f"Missing or unexpected role MCP server: {path}")
    for server, section in servers.items():
        transport = project.get(server, {})
        require(transport.get("command") or transport.get("url"),
                f"Missing project MCP transport: {server}")
        require(server in vscode, f"Missing VS Code MCP server: {server}")
        for key in ("command", "args", "url", "cwd", "env", "env_vars", "bearer_token_env_var",
                    "startup_timeout_sec"):
            require(section.get(key) == transport.get(key),
                    f"Role MCP transport differs from project config: {path} -> {server}.{key}")
        enabled = server in expected
        require(section.get("enabled") is enabled and section.get("required") is False,
                f"Wrong role MCP activation/fallback: {path} -> {server}")
        require(not section.get("disabled_tools"), f"Role tools unexpectedly disabled: {path}")
        names = section.get("enabled_tools", [])
        require(isinstance(names, list) and all(isinstance(name, str) for name in names),
                f"Invalid MCP allowlist: {path} -> {server}")
        require(len(names) == len(set(names)) and set(names) == expected.get(server, set()),
                f"Missing, unknown, or unsafe MCP tools: {path} -> {server}")


def frontmatter(path):
    try:
        import yaml
    except ImportError as error:
        raise RuntimeError("PyYAML is required: python -m pip install PyYAML") from error
    content = path.read_text(encoding="utf-8-sig")
    header = re.match(r"\A---\s*\n(.*?)\n---", content, re.S)
    require(header is not None, f"Missing YAML frontmatter: {path}")
    try:
        metadata = yaml.safe_load(header.group(1))
    except yaml.YAMLError as error:
        raise RuntimeError(f"Invalid YAML frontmatter: {path}: {error}") from error
    require(isinstance(metadata, dict), f"Frontmatter must be a mapping: {path}")
    return metadata, content[header.end():]


def references(root, source, text, expected):
    targets = set(re.findall(
        r"(?<![\w/])(?:AGENTS\.md|(?:\.agents|docs)/(?:[\w.-]+/)*[\w.-]+\.(?:md|json|toml))",
        text))
    require(expected <= targets, f"Missing instruction routes in {source}: {expected - targets}")
    for target in targets:
        require((root / target).is_file(), f"Broken adapter reference: {source} -> {target}")


def check_role_routes(source, text, role):
    # These are instruction-presence checks, not proof of runtime behavior.
    require("Task contract" in text, f"Missing task contract route: {source}")
    if role == "implementator":
        require("only when the user explicitly selects or requests custom Implementator" in text,
                f"Missing explicit Implementator invocation guard: {source}")
        require("Load the installed user-level software-design-principles skill" in text,
                f"Missing user-level design skill route: {source}")
    elif role == "researcher":
        require("Bounded research" in text, f"Missing shared research route: {source}")
        require(".agents/implementation/IMPLEMENTATION.md" not in text,
                f"Researcher must not load Implementator workflow: {source}")


def check(root):
    project = tomllib.loads((root / ".codex/config.toml").read_text(encoding="utf-8-sig"))["mcp_servers"]
    vscode = json.loads((root / ".vscode/mcp.json").read_text(encoding="utf-8-sig"))["servers"]
    fetch = project.get("fetch", {})
    require(fetch.get("command") == "uvx"
            and "mcp-server-fetch" in fetch.get("args", [])
            and fetch.get("env", {}).get("PYTHONIOENCODING") == "utf-8",
            "Missing Fetch launch settings or Windows encoding.")
    require("fetch" in vscode, "Missing VS Code Fetch server.")
    for key in ("command", "args", "env"):
        require(vscode["fetch"].get(key) == fetch.get(key),
                f"Fetch transport differs from project config: VS Code.{key}")
    require(vscode["fetch"].get("type") == "stdio", "Invalid VS Code Fetch transport type.")
    required = {
        "implementator": {"AGENTS.md", ".agents/implementation/IMPLEMENTATION.md", ".agents/review/REVIEW.md",
                          ".agents/instructions/tooling.md"},
        "reviewer": {"AGENTS.md", ".agents/review/REVIEW.md",
                     ".agents/instructions/tooling.md", ".agents/skills/open-code-review-delegate/SKILL.md"},
        "researcher": {"AGENTS.md", ".agents/instructions/tooling.md"},
    }
    for role, targets in required.items():
        path = root / f".codex/agents/{role}.toml"
        data = tomllib.loads(path.read_text(encoding="utf-8-sig"))
        require(data.get("name") == role and data.get("description"), f"Invalid Codex role: {path}")
        references(root, path, data["developer_instructions"], targets)
        check_role_routes(path, data["developer_instructions"], role)
        check_mcp(root, path, data, role, project, vscode)
        require(data.get("sandbox_mode") == "danger-full-access",
                f"Custom agent must use the effective unrestricted shell/edit environment: {path}")
        if role == "researcher":
            require(data.get("agents", {}).get("enabled") is False,
                    f"Researcher delegation must be disabled: {path}")
        else:
            require(data.get("agents", {}).get("enabled") is not False,
                    f"Implementation/Reviewer delegation unexpectedly disabled: {path}")

        adapter = root / ".github/agents" / f"{role}.agent.md"
        header, body = frontmatter(adapter)
        require(header.get("name") == role and header.get("description"),
                f"Invalid role: {adapter}")
        references(root, adapter, body, targets)
        check_role_routes(adapter, body, role)
        tools = header.get("tools")
        require(isinstance(tools, list), f"Tools must be a YAML list: {adapter}")
        require(header.get("target") == "vscode", f"Local target missing: {adapter}")
        if role == "implementator":
            require(header.get("user-invocable") is True
                    and header.get("disable-model-invocation") is True,
                    f"Implementator must require explicit user invocation: {adapter}")
        permitted = {"read", "search", "web", "edit", "execute"}
        if role != "researcher":
            permitted.add("agent")
        permitted.update(f"{server}/{name}" for server, names in role_mcp_tools(role).items()
                         for name in names)
        require({"read", "search"} <= set(tools), f"Read/search tools missing: {adapter}")
        require(all(isinstance(tool, str) for tool in tools)
                and len(tools) == len(set(tools)) and set(tools) == permitted,
                f"Missing, unknown, duplicate, or unsafe adapter tool: {adapter}")
        if role == "researcher":
            require("agent" not in tools, f"Researcher can delegate: {adapter}")
            require(header.get("agents") == [], f"Researcher allowlist is not empty: {adapter}")
        elif role == "reviewer":
            require("agent" in tools, f"Reviewer helper tool missing: {adapter}")
            require(set(header.get("agents", [])) == {"reviewer", "researcher"},
                    f"Reviewer may invoke only reader roles: {adapter}")

    manifest_path = root / ".opencodereview/rule.json"
    manifest = json.loads(manifest_path.read_text(encoding="utf-8-sig"))
    for key in ("include", "exclude"):
        require(isinstance(manifest.get(key), list) and all(isinstance(x, str) for x in manifest[key]),
                f"Invalid OCR {key} patterns.")
    require("**" not in manifest["include"] and "**/*" not in manifest["include"],
            "A blanket OCR include bypasses important default filters.")
    rules = manifest.get("rules")
    require(isinstance(rules, list) and len(rules) == 5, "Expected four project routes and one fallback.")
    for rule in rules:
        target = (root / rule["rule"]).resolve()
        require(target.is_relative_to(root) and target.is_file(), f"Invalid OCR rule reference: {rule}")
        require(rule.get("merge_system_rule") is False, f"Unexpected built-in rule merge: {rule}")
    samples = {
        "CsharpTestAutomation.Framework/API/Clients/A.cs": "framework.md",
        "CsharpTestAutomation.Tests/Steps/API/NetBox/A.cs": "nunit.md",
        "CsharpTestAutomation.Tests/UI/Pages/A.cs": "nunit.md",
        "CsharpTestAutomation.Bdd.Tests/Features/A.feature": "bdd.md",
        "CsharpTestAutomation.Bdd.Tests/Steps/UI/NetBox/A.cs": "bdd.md",
        "CsharpTestAutomation.Framework.Test/A.cs": "framework-tests.md",
        "docs/A.md": "repository.md", ".codex/agents/reviewer.toml": "repository.md",
        ".github/agents/reviewer.agent.md": "repository.md",
        ".opencodereview/rule.json": "repository.md",
    }
    for path, filename in samples.items():
        matched = next((r for r in rules if fnmatch.fnmatchcase(path, r["path"])), None)
        require(matched is not None and Path(matched["rule"]).name == filename,
                f"Wrong first-match OCR route: {path}")
    for path in [*root.glob(".codex/agents/*.toml"), *root.glob(".agents/scripts/*"), manifest_path]:
        if path.is_file():
            for line_number, line in enumerate(path.read_text(encoding="utf-8-sig").splitlines(), 1):
                require(line.rstrip(" \t") == line, f"Trailing whitespace: {path}:{line_number}")
    print(f"Checked six adapters, MCP transports and role allowlists, instruction references, shell/edit access, role boundaries, five OCR rule references, and {len(samples)} OCR routes.")


def negative_fixtures(root):
    adapters = [*root.glob(".codex/agents/*.toml"), *root.glob(".github/agents/*.agent.md")]
    files = {root / "AGENTS.md", root / ".opencodereview/rule.json",
             root / ".codex/config.toml", root / ".vscode/mcp.json",
             *adapters, *root.glob(".agents/review/rules/*.md")}
    for adapter in adapters:
        for target in re.findall(
                r"(?<![\w/])(?:AGENTS\.md|(?:\.agents|docs)/(?:[\w.-]+/)*[\w.-]+\.(?:md|json|toml))",
                adapter.read_text(encoding="utf-8")):
            files.add(root / target)
    mutations = [
        (".codex/agents/researcher.toml", "enabled = false", "enabled = true", "Researcher delegation"),
        (".codex/agents/reviewer.toml", 'sandbox_mode = "danger-full-access"', 'sandbox_mode = "workspace-write"', "Reviewer restricted sandbox"),
        (".codex/agents/researcher.toml", 'sandbox_mode = "danger-full-access"', 'sandbox_mode = "read-only"', "Researcher restricted sandbox"),
        (".github/agents/reviewer.agent.md", "'execute'", "'exec'", "VS Code invalid shell tool"),
        (".codex/agents/reviewer.toml", 'name = "reviewer"', 'name = [', "Malformed TOML"),
        (".github/agents/researcher.agent.md", ".agents/instructions/tooling.md", ".agents/instructions/missing.md", "Plain adapter reference"),
        (".opencodereview/rule.json", ".agents/review/rules/framework.md", ".agents/review/rules/missing.md", "Manifest rule reference"),
        (".opencodereview/rule.json", '"include": [', '"include": [invalid', "Malformed JSON"),
        (".codex/agents/reviewer.toml", 'enabled = false', 'enabled = true', "Reader browser tools"),
        (".codex/agents/reviewer.toml", '"list_projects"', '"index_repository", "list_projects"', "Reader MCP mutation"),
        (".codex/agents/reviewer.toml", '["find_files", "grep", "multi_grep"]', '["*"]', "Reader MCP wildcard"),
        (".codex/agents/reviewer.toml", 'command = "powershell.exe"', 'command = "cmd.exe"', "Broken role transport"),
        (".codex/agents/researcher.toml", '"resolve-library-id"', '"resolve_library_id"', "Normalized MCP alias"),
        (".github/agents/reviewer.agent.md", "'fff/find_files'", "'chrome-devtools/evaluate_script'", "VS Code unsafe MCP tool"),
        (".github/agents/reviewer.agent.md", "'fff/find_files'", "'firecrawl/firecrawl_scrape'", "Reviewer Firecrawl tool"),
        (".github/agents/reviewer.agent.md", "'fff/find_files'", "'fff/*'", "VS Code MCP wildcard"),
        (".github/agents/researcher.agent.md", "'read'", "'read'\n  - 'agent'", "VS Code Researcher delegation"),
        (".vscode/mcp.json", '"fff":', '"missing-fff":', "Missing VS Code server binding"),
        (".vscode/mcp.json", '"fetch":', '"missing-fetch":', "Missing VS Code Fetch server"),
        (".codex/agents/researcher.toml", 'enabled_tools = ["fetch"]', 'enabled_tools = ["*"]', "Fetch tool wildcard"),
        (".codex/agents/reviewer.toml", 'PYTHONIOENCODING = "utf-8"', 'PYTHONIOENCODING = "ascii"', "Fetch role encoding drift"),
        (".github/agents/reviewer.agent.md", "'fetch/fetch'", "'fetch/*'", "VS Code Fetch wildcard"),
        (".github/agents/implementator.agent.md", "disable-model-invocation: true", "disable-model-invocation: false", "Automatic Implementator invocation"),
        (".github/agents/implementator.agent.md", "user-invocable: true", "user-invocable: false", "Implementator not user selectable"),
    ]
    for directory, suffix in ((".codex/agents", ".toml"),
                              (".github/agents", ".agent.md")):
        mutations.extend([
            (f"{directory}/implementator{suffix}",
             "Load the installed user-level software-design-principles skill",
             "Load another skill", f"{directory} user-level design skill route"),
            (f"{directory}/implementator{suffix}",
             "only when the user explicitly selects or requests custom Implementator",
             "for any implementation request", f"{directory} invocation guard"),
            (f"{directory}/researcher{suffix}", "Bounded research", "Other guidance",
             f"{directory} shared research route"),
            (f"{directory}/researcher{suffix}", "Bounded research",
             "Bounded research and .agents/implementation/IMPLEMENTATION.md",
             f"{directory} Researcher implementation coupling"),
            (f"{directory}/reviewer{suffix}", "Task contract", "Other guidance",
             f"{directory} Reviewer contract route"),
        ])
    with disposable_repository("configuration-harness-") as repo:
        for source in files:
            target = repo / source.relative_to(root)
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(source, target)
        with contextlib.redirect_stdout(io.StringIO()):
            check(repo)
        for relative, old, new, label in mutations:
            target = repo / relative
            original = target.read_text(encoding="utf-8")
            require(old in original, f"Negative fixture no longer applies: {label}")
            target.write_text(original.replace(old, new, 1), encoding="utf-8")
            rejected = False
            try:
                with contextlib.redirect_stdout(io.StringIO()):
                    check(repo)
            except (RuntimeError, ValueError, KeyError, TypeError, OSError):
                rejected = True
            finally:
                target.write_text(original, encoding="utf-8")
            require(rejected, f"Configuration checker accepted invalid fixture: {label}")
    print(f"Rejected all {len(mutations)} invalid configuration fixtures in an isolated copy.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repo", type=Path, default=Path(__file__).resolve().parents[2])
    args = parser.parse_args()
    try:
        check(args.repo.resolve())
        negative_fixtures(args.repo.resolve())
    except (RuntimeError, ValueError, KeyError, TypeError, OSError) as error:
        print(f"FAILED: {error}", file=sys.stderr)
        sys.exit(1)
