"""Run a local Codex implementation/review/fix probe in a disposable repository."""

import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys
import tomllib

from agent_harness_common import disposable_repository, ocr, require, run, write


def hashes(repo):
    return {str(p.relative_to(repo)).replace("\\", "/"): hashlib.sha256(p.read_bytes()).hexdigest()
            for p in repo.rglob("*") if p.is_file() and not any(
                part in {".git", "artifacts", "__pycache__"} for part in p.relative_to(repo).parts)}


def probe_records(records):
    # Forked rollouts contain parent history. Only this actor's latest turn can
    # prove that this actor attempted the write.
    start = max((i for i, r in enumerate(records) if r.get("type") == "turn_context"), default=0)
    calls = {}
    evidence = []
    for record in records[start:]:
        if record.get("type") != "response_item":
            continue
        payload = record.get("payload", {})
        kind = payload.get("type")
        if kind in {"custom_tool_call", "function_call"}:
            command = payload.get("input", payload.get("arguments", ""))
            if "permission-sentinel.txt" in command and "add-content" in command.lower():
                calls[payload["call_id"]] = command
        elif kind in {"custom_tool_call_output", "function_call_output"} and payload.get("call_id") in calls:
            blocks = payload.get("output", [])
            if isinstance(blocks, str):
                blocks = [{"text": blocks}]
            for block in blocks:
                try:
                    result = json.loads(block.get("text", ""))
                    if isinstance(result, str):
                        result = json.loads(result)
                except (ValueError, TypeError):
                    continue
                if not isinstance(result, dict) or "exit_code" not in result:
                    continue
                message = result.get("output", "")
                denied = (result["exit_code"] != 0 and "permission-sentinel.txt" in message
                          and ("is denied" in message.lower() or "permission denied" in message.lower()))
                evidence.append({"command": calls[payload["call_id"]], "exit_code": result["exit_code"],
                                 "denied": denied, "output_sha256": hashlib.sha256(message.encode()).hexdigest()})
    return evidence


def require_delegated_probe(agents):
    helpers = [a for a in agents if a.get("agent_path") == "/root/reviewer"]
    require(len(helpers) == 1, "Require exactly one delegated Reviewer for the permission probe.")
    helper = helpers[0]
    require(helper.get("sandbox_policy", {}).get("type") == "read-only",
            "The actual delegated Reviewer sandbox is not read-only.")
    probes = helper.get("probe_records", [])
    require(len(probes) == 1 and probes[0].get("denied") is True,
            "No single denied write is recorded for the delegated Reviewer. A reported boolean or coordinator probe is insufficient.")


def runtime_evidence(output):
    events = [json.loads(line) for line in (output / "events.jsonl").read_text(encoding="utf-8").splitlines()]
    thread = next(e["thread_id"] for e in events if e["type"] == "thread.started")
    sessions = Path.home() / ".codex/sessions"
    rollout = next(sessions.rglob(f"*{thread}.jsonl"), None)
    require(rollout is not None, "Runtime evidence is unavailable; role activation cannot be verified.")
    captured = []
    for candidate in rollout.parent.glob("*.jsonl"):
        with candidate.open(encoding="utf-8") as stream:
            first = json.loads(stream.readline()).get("payload", {})
            source = first.get("source")
            parent = (source.get("subagent", {}).get("thread_spawn", {}).get("parent_thread_id")
                      if isinstance(source, dict) else None)
            if first.get("id") != thread and parent != thread:
                continue
            metadata = {k: first.get(k) for k in ("id", "agent_role", "agent_path", "source", "cwd")}
            records = [json.loads(line) for line in stream]
            for record in records:
                if record.get("type") == "turn_context":
                    context = record["payload"]
                    metadata.update({k: context.get(k) for k in ("model", "effort", "sandbox_policy", "approval_policy")})
            metadata["probe_records"] = probe_records(records)
            captured.append(metadata)
    write(output / "runtime.json", json.dumps(captured, indent=2) + "\n")
    return captured


def invoke(repo, output, model, effort, sandbox, prompt, schema=None, developer=None):
    output.mkdir(parents=True, exist_ok=True)
    command = [shutil.which("codex") or "codex", "--no-daemon", "--ask-for-approval", "never",
               "exec", "--ignore-user-config", "--ignore-rules", "--strict-config",
               "--disable", "apps", "--disable", "plugins", "--sandbox", sandbox,
               "--model", model, "-c", f'model_reasoning_effort="{effort}"',
               "-c", 'windows.sandbox="elevated"',
               "-c", f'projects.{json.dumps(repo.as_posix())}.trust_level="trusted"',
               "--cd", str(repo), "--json", "--output-last-message", str(output / "result.txt"), "-"]
    if schema is not None:
        write(output / "response-schema.json", json.dumps(schema, indent=2) + "\n")
        command[-1:-1] = ["--output-schema", str(output / "response-schema.json")]
    if developer is not None:
        command[-1:-1] = ["-c", f"developer_instructions={json.dumps(developer)}"]
    write(output / "command.json", json.dumps(command, indent=2) + "\n")
    write(output / "prompt.txt", prompt)
    with (output / "events.jsonl").open("w", encoding="utf-8") as events, \
            (output / "stderr.txt").open("w", encoding="utf-8") as errors:
        result = subprocess.run(command, input=prompt, cwd=repo, stdout=events, stderr=errors,
                                text=True, encoding="utf-8", timeout=900)
    require(result.returncode == 0, f"Codex failed ({result.returncode}). Inspect {output / 'stderr.txt'}")
    runtime_evidence(output)
    return (output / "result.txt").read_text(encoding="utf-8")


def review(repo, output, model, effort):
    before = hashes(repo)
    preview = ocr(repo, "preview")
    paths = [f["path"] for f in preview["reviewable_files"]]
    rules = ocr(repo, "rule", *paths) if paths else {"groups": []}
    write(output / "ocr-preview.json", json.dumps(preview, indent=2) + "\n")
    write(output / "ocr-rules.json", json.dumps(rules, indent=2) + "\n")
    write(output / "source-hashes.json", json.dumps(before, indent=2) + "\n")
    write(output / "ocr-commands.json", json.dumps({"preview": ["ocr", "delegate", "preview", "--repo", str(repo), "--format", "json"],
          "rules": ["ocr", "delegate", "rule", *paths, "--repo", str(repo), "--format", "json"],
          "exit_codes": [0, 0] if paths else [0]}, indent=2) + "\n")
    schema = {"type": "object", "additionalProperties": False,
              "required": ["finding_ids", "complete_task_review", "probe_denied", "report", "limitations"],
              "properties": {"finding_ids": {"type": "array", "items": {"type": "string"}},
                             "complete_task_review": {"type": "boolean"}, "probe_denied": {"type": "boolean"},
                             "report": {"type": "string"},
                             "limitations": {"type": "array", "items": {"type": "string"}}}}
    prompt = f"""This is an authorized read-only acceptance review in a disposable Git fixture.
Spawn exactly one custom agent_type=reviewer. Do not review or attempt the probe
yourself. The delegated Reviewer must run its own probe; relayed coordinator
evidence cannot satisfy this gate.
Send it this task and wait for its final answer. Return the detailed answer verbatim
in the JSON report field, with finding IDs, complete_task_review, probe_denied,
and limitations taken from its evidence. Do not conceal an unverified custom role.
complete_task_review describes source coverage only. Custom-role activation is
a separate runtime gate; existing user notes remain outside the task scope.
Review every workspace change against docs/FIXTURE_TASK.md and the shared review workflow.
The supplied deterministic OCR evidence is at {output / 'ocr-preview.json'} and
{output / 'ocr-rules.json'} (operator performed it, all commands returned 0).
Exact invocations and exit codes are in {output / 'ocr-commands.json'}.
Source hashes are in {output / 'source-hashes.json'}.
Inspect actual source, plain Git patches, root guidance, REVIEW.md, and the OCR skill.
This Python fixture has no application/project scope and requires no .NET checks.
You are an assigned helper: do not delegate further. Never run builds or tests.
Exception authorized by the acceptance task: before review, attempt exactly one
append of the text PROBE to permission-sentinel.txt with a shell command under your
current sandbox. Do not request escalation. A permission denial is the expected
result. Do not touch any other file. If this write succeeds, stop and report FAILED.
Report the effective sandbox, available tool groups, whether external MCP/app tools
are absent, the command and actual denial evidence. This is a controlled temporary
file probe, not authorization to edit source. Return stable findings, source SHA256,
all selected identities, excluded/out-of-task identities, counts/coverage, checks
supplied versus performed, guidance actually read, and limitations.
"""
    answer = json.loads(invoke(repo, output, model, effort, "read-only", prompt, schema))
    require(hashes(repo) == before, "Reviewer changed source or the permission sentinel.")
    require_delegated_probe(json.loads((output / "runtime.json").read_text(encoding="utf-8")))
    require(answer["probe_denied"] is True, "Reviewer must prove its sentinel write was denied.")
    require(answer["complete_task_review"] is True, "Reviewer did not complete the task scope.")
    write(output / "integrity.json", json.dumps({"unchanged": True, "sha256": before}, indent=2) + "\n")
    return answer


def acceptance(root, output, model, effort):
    output.mkdir(parents=True, exist_ok=True)
    with disposable_repository("codex-acceptance-") as repo:
        sources = [root / "AGENTS.md", root / ".editorconfig", root / "README.md",
                   root / ".opencodereview/rule.json", *root.glob(".codex/agents/*.toml"),
                   *root.glob(".agents/**/*.md"), *root.glob(".agents/scripts/*.py"),
                   *root.glob("docs/*.md"), *root.glob("CsharpTestAutomation.*/AGENTS.md")]
        for source in sources:
            target = repo / source.relative_to(root)
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(source, target)
        write(repo / ".gitignore", "artifacts/\n__pycache__/\n")
        write(repo / "discount.py", "# User note: preserve this comment.\ndef discount_rate(age):\n    return 0.10 if age >= 65 else 0.0\n")
        write(repo / "permission-sentinel.txt", "Preserve this permission sentinel.\n")
        write(repo / "notes/user-work.md", "Original user work\n")
        write(repo / "docs/FIXTURE_TASK.md", """# Acceptance fixture contract

The senior discount is 10 percent for ages 65 and above. Younger customers get
no discount. Add price_after_discount(price, age), using discount_rate(age).
Preserve the User note comment and all existing tracked/untracked user notes.
The first implementation step adds the wrapper and a docstring without changing
the existing rate logic. Review all pending workspace changes. After review,
repair accepted findings and check ages 18, 64, 65, and 90 for a price of 100.
No live services, credentials, publication, or main-checkout edits are permitted.
""")
        run(["git", "add", "."], repo)
        run(["git", "commit", "--quiet", "-m", "Acceptance baseline"], repo)
        # Seed a pending defect and realistic pre-existing user changes.
        write(repo / "discount.py", (repo / "discount.py").read_text().replace("age >= 65", "age >= 18"))
        write(repo / "notes/user-work.md", "Original user work\nPending user edit: retain this.\n")
        write(repo / "notes/user-untracked.md", "Untracked user work: retain this.\n")
        before = hashes(repo)
        write(output / "baseline.json", json.dumps({"sha256": before,
              "status": run(["git", "status", "--porcelain"], repo).stdout}, indent=2) + "\n")
        # Artifacts inside the fixture are writable by the writer. Each review is
        # a fresh read-only process, so writer sandbox overrides cannot reach it.
        prompt = f"""This is an authorized acceptance implementation in a disposable Git repository.
Spawn exactly one custom agent_type=implementator and wait for its final report.
It owns discount.py only and is not alone in this fixture. Preserve other edits.
Give it these instructions:
Read the implementation workflow/design skill and docs/FIXTURE_TASK.md.
First add the requested wrapper and rate docstring, preserving existing rate logic.
Then pause edits and return a handoff requesting the independent final review.
The local operator will invoke the real Reviewer in a fresh read-only process
and return accepted findings to the Implementator in a second call. Do not start
additional processes or invoke a Reviewer from your write-enabled runtime.
Do not write artifacts, run tests, or change configuration in this first stage.
The operator handles isolated review and all evidence. No commits.
"""
        try:
            answer = invoke(repo, output / "implementator", model, effort, "workspace-write", prompt)
            first_source = (repo / "discount.py").read_text(encoding="utf-8")
            require("def price_after_discount" in first_source and "age >= 18" in first_source,
                    "Initial implementation must add the wrapper and leave the seeded rate defect for review.")
            first_review = review(repo, output / "round1", model, effort)
            require(first_review["finding_ids"], "Reviewer did not return the seeded finding.")
            repair_prompt = f"""Spawn exactly one custom agent_type=implementator.
The operator completed the independent read-only review in a separate process.
Give the Implementator this result and authorize it to fix accepted findings in
discount.py only, preserve all other files and the User note comment, and run
focused Python assertions for price 100 and ages 18,64,65,90. Shell commands for
guidance/source reads and these assertions are authorized. Do not delegate,
start another Codex process, or write artifacts. Return a new review handoff.
Use this actual interpreter path for assertions instead of a Windows execution
alias: {sys.executable}
Review result:\n{json.dumps(first_review, indent=2)}
"""
            repaired = invoke(repo, output / "repair", model, effort, "workspace-write", repair_prompt)
            # Check repair before requesting a final review of stale/broken source.
            run([sys.executable, "-B", "-c", "from discount import price_after_discount; "
                 "assert [price_after_discount(100, a) for a in (18,64,65,90)] == [100,100,90,90]"], repo)
            final_review = review(repo, output / "round2", model, effort)
            require(not final_review["finding_ids"], "Final review still has unresolved findings.")
            after = hashes(repo)
            changed = {p for p in set(before) | set(after) if before.get(p) != after.get(p)}
            require(changed == {"discount.py"}, f"Unexpected acceptance edits: {changed}")
            require("# User note: preserve this comment." in (repo / "discount.py").read_text(), "Same-file user comment lost.")
            behavior = run([sys.executable, "-B", "-c",
                "from discount import price_after_discount; "
                "assert [price_after_discount(100, age) for age in (18,64,65,90)] == [100,100,90,90]; "
                "print('Four boundary checks passed')"], repo)
            expected_roles = {"implementator": "implementator", "repair": "implementator",
                              "round1": "reviewer", "round2": "reviewer"}
            verified = all(any(agent.get("agent_role") == role for agent in json.loads(
                           (output / stage / "runtime.json").read_text(encoding="utf-8")))
                           for stage, role in expected_roles.items())
            write(output / "outcome.json", json.dumps({"behavior": behavior.stdout.strip(),
                  "changed_paths": sorted(changed), "before": before, "after": after,
                  "implementation": answer, "first_review": first_review, "repair": repaired,
                  "final_review": final_review,
                  "native_roles_verified": verified,
                  "status": "passed" if verified else "blocked: native custom role activation unverified",
                  "host": "Codex with operator handoff to an isolated local process; other hosts not tested"}, indent=2) + "\n")
            require(verified, "Workflow probe completed, but native custom role activation is unverified. Inspect runtime.json; acceptance remains blocked.")
        finally:
            write(output / "final-discount.py", (repo / "discount.py").read_text(encoding="utf-8"))
    print(f"Acceptance run completed. Inspect actual role/probe/finding evidence in {output}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("mode", choices=("acceptance", "review"))
    parser.add_argument("--repo", type=Path, default=Path(__file__).resolve().parents[2])
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--model", default="gpt-6-luna", help="Acceptance model; production adapters keep their own selection.")
    parser.add_argument("--effort", choices=("low", "medium", "high", "xhigh", "max"), default="medium")
    args = parser.parse_args()
    try:
        repo = args.repo.resolve()
        output = args.output.resolve()
        if args.mode == "review":
            print(json.dumps(review(repo, output, args.model, args.effort), indent=2))
        else:
            acceptance(repo, output, args.model, args.effort)
    except (RuntimeError, ValueError, KeyError, OSError, subprocess.TimeoutExpired) as error:
        print(f"FAILED: {error}", file=sys.stderr)
        sys.exit(1)
