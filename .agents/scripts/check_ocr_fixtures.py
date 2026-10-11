"""Exercise the real OCR delegate CLI in disposable Git repositories (no LLM)."""

import argparse
from collections import Counter
import hashlib
import json
from pathlib import Path
import shutil
import sys

from agent_harness_common import disposable_repository, ocr, ocr_command, require, run, write


def check(root, output):
    evidence = {"ocr_version": run([*ocr_command(), "--version"], root).stdout.strip(),
                "manifest_sha256": hashlib.sha256(
                    (root / ".opencodereview/rule.json").read_bytes()).hexdigest(), "cases": {}}
    manifest = json.loads((root / ".opencodereview/rule.json").read_text(encoding="utf-8"))
    with disposable_repository("ocr-harness-") as repo:
        for source in [root / ".opencodereview/rule.json", *root.glob(".agents/review/rules/*.md")]:
            target = repo / source.relative_to(root)
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(source, target)
        initial = {".vscode/settings.json": "{}\n", "fixtures/modified.md": "old\n", "fixtures/deleted.md": "old deleted\n",
                   "fixtures/old-name.md": "rename content\n" * 20}
        for path, content in initial.items():
            write(repo / path, content)
        run(["git", "add", "."], repo)
        run(["git", "commit", "--quiet", "-m", "Fixture baseline"], repo)
        baseline = run(["git", "rev-parse", "HEAD"], repo).stdout.strip()
        empty = ocr(repo, "preview")
        require(empty["total_files"] == 0 and empty["reviewable_count"] == 0,
                "Clean workspace must report zero files.")
        evidence["cases"]["zero_files"] = empty

        additions = {}
        for pattern in manifest["include"]:
            require(pattern.startswith("**/*.") or pattern in {".editorconfig", ".gitignore"},
                    f"Add an explicit include fixture for the new pattern: {pattern}")
            path = "fixtures/include/sample." + pattern[5:] if pattern.startswith("**/*.") else pattern
            additions[path] = "# Harmless include sentinel\n"
        additions.update({"CsharpTestAutomation.Framework/API/A.cs": "// Framework\n",
                          "CsharpTestAutomation.Tests/UI/A.cs": "// NUnit\n",
                          "CsharpTestAutomation.Bdd.Tests/Features/A.feature": "Feature: Fixture\n",
                          "CsharpTestAutomation.Framework.Test/A.cs": "// Self-test\n",
                          ".codex/agents/example.toml": 'name = "sentinel"\n',
                          ".github/agents/example.agent.md": "# Agent sentinel\n"})
        exclusions = {}
        for pattern in manifest["exclude"]:
            path = pattern.replace("**/", "fixtures/").replace("**", "sample.md").replace("*", "sample")
            exclusions[path] = "Harmless exclusion sentinel, never a credential\n"
        # Default provider and credential filters must survive include overrides.
        exclusions.update({".env": "SENTINEL=harmless\n",
                           "credentials.json": "{}\n", "id_rsa": "Harmless sentinel\n",
                           "CsharpTestAutomation.Tests/appsettings.local.json": "{}\n",
                           "CsharpTestAutomation.Bdd.Tests/appsettings.local.json": "{}\n",
                           "fixtures/unsupported.harnessfixture": "Unsupported extension sentinel\n"})
        write(repo / "fixtures/modified.md", "new\n")
        write(repo / ".vscode/settings.json", '{"fixture": true}\n')
        (repo / "fixtures/deleted.md").unlink()
        run(["git", "mv", "fixtures/old-name.md", "fixtures/new-name.md"], repo)
        for path, content in {**additions, **exclusions}.items():
            write(repo / path, content)

        def preview_case(name, arguments, added_status):
            preview = ocr(repo, "preview", *arguments)
            selected = Counter((f["path"], f["status"]) for f in preview["reviewable_files"])
            excluded = Counter((f["path"], f["status"]) for f in preview["excluded_files"])
            expected = Counter({(p, added_status): 1 for p in additions})
            expected.update({("fixtures/modified.md", "modified"): 1,
                             ("fixtures/new-name.md", "renamed"): 1})
            require(selected == expected, f"{name}: unexpected selection: {selected - expected}; missing: {expected - selected}")
            expected_excluded = Counter({(p, added_status): 1 for p in exclusions})
            expected_excluded.update({("fixtures/deleted.md", "deleted"): 1})
            expected_excluded.update({(".vscode/settings.json", "modified"): 1})
            require(excluded == expected_excluded, f"{name}: exclusion identity mismatch: {excluded}; expected: {expected_excluded}")
            for item in preview["excluded_files"]:
                reason = ("deleted" if item["status"] == "deleted" else
                          "provider_directory" if item["path"].startswith(".vscode/") else
                          "secret_exclude" if item["path"] in {".env", "id_rsa"} else
                          "unsupported_ext" if item["path"].endswith(".harnessfixture") else
                          "user_exclude")
                require(item.get("exclude_reason") == reason,
                        f"{name}: wrong exclusion reason: {item}; expected {reason}.")
            require(preview["total_files"] == sum(selected.values()) + sum(excluded.values()),
                    f"{name}: total identity count mismatch.")
            require(preview["reviewable_count"] == sum(selected.values()) and
                    preview["excluded_count"] == sum(excluded.values()), f"{name}: count mismatch.")
            # OCR reports the new path only; Git supplies the old-path evidence.
            git_args = (["show", "--format=", "--name-status", arguments[1]] if name == "commit"
                        else ["diff", "--name-status", f"{baseline}..{arguments[3]}"] if name == "range"
                        else ["diff", "--name-status", "HEAD"])
            changes = run(["git", "--no-pager", *git_args], repo).stdout
            require("R100\tfixtures/old-name.md\tfixtures/new-name.md" in changes,
                    f"{name}: old/new rename evidence missing from Git.")
            evidence["cases"][name] = {"arguments": arguments, "exit_code": 0,
                                      "preview": preview, "git_name_status": changes}
            return preview

        preview_case("workspace", [], "added")
        run(["git", "add", "."], repo)
        run(["git", "commit", "--quiet", "-m", "Fixture changes"], repo)
        changed = run(["git", "rev-parse", "HEAD"], repo).stdout.strip()
        commit = preview_case("commit", ["--commit", changed], "added")
        require(commit["mode"] == "commit" and commit["commit"] == changed, "Commit metadata mismatch.")
        # Diverging source ref proves range selection uses merge-base, not source tip.
        run(["git", "checkout", "--quiet", "-b", "diverged", baseline], repo)
        write(repo / "source-only.md", "Source branch change\n")
        run(["git", "add", "."], repo)
        run(["git", "commit", "--quiet", "-m", "Diverging source"], repo)
        source_tip = run(["git", "rev-parse", "HEAD"], repo).stdout.strip()
        range_result = preview_case("range", ["--from", source_tip, "--to", changed], "added")
        require(range_result["mode"] == "range" and range_result["merge_base"] == baseline,
                "Range must identify the merge-base on diverging branches.")
        run(["git", "checkout", "--quiet", "--detach", changed], repo)
        duplicate = "fixtures/modified.md"
        run(["git", "rm", "--quiet", duplicate], repo)
        write(repo / duplicate, "Recreated untracked source\n")
        recreated = ocr(repo, "preview")
        require(Counter((f["path"], f["status"]) for f in recreated["reviewable_files"])
                == Counter({(duplicate, "added"): 1}), "Recreated source must remain reviewable.")
        require(Counter((f["path"], f["status"]) for f in recreated["excluded_files"])
                == Counter({(duplicate, "deleted"): 1}), "Staged deletion must remain a separate identity.")
        require(recreated["total_files"] == 2, "Duplicate path identity must not be collapsed.")
        evidence["cases"]["staged_deleted_recreated"] = recreated

        paths = [*additions, "fixtures/modified.md", "fixtures/new-name.md"]
        groups = ocr(repo, "rule", *paths)
        grouped = [p for group in groups["groups"] for p in group["files"]]
        require(Counter(grouped) == Counter(paths), "Rules must account for every selected path once.")
        require(len(groups["groups"]) == 5, "Expected four specific project rule groups and fallback.")
        for group in groups["groups"]:
            rule = next((r for r in manifest["rules"] if r["path"] == group["pattern"]), None)
            require(rule is not None and group["source"] == "project", "Unexpected rule source/pattern.")
            require(group["rule"].strip() == (root / rule["rule"]).read_text(encoding="utf-8").strip(),
                    "OCR resolved different rule content.")
            for path in group["files"]:
                expected_rule = next(r for r in manifest["rules"]
                                     if path.startswith(r["path"].removesuffix("**")) or r["path"] == "**")
                require(rule == expected_rule, f"Wrong specific/fallback rule for {path}.")
        evidence["cases"]["rules"] = groups
    output.mkdir(parents=True, exist_ok=True)
    write(output / "ocr-fixtures.json", json.dumps(evidence, indent=2) + "\n")
    print(f"OCR passed: workspace/range/commit, zero files, four Git statuses, duplicate path identities, {len(additions)} include/routing sentinels, {len(exclusions)} exclusion sentinels, five rule groups. Evidence: {output}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--repo", type=Path, default=Path(__file__).resolve().parents[2])
    args = parser.parse_args()
    try:
        check(args.repo.resolve(), args.repo.resolve() / "artifacts/agent-harness")
    except (RuntimeError, ValueError, KeyError, OSError) as error:
        print(f"FAILED: {error}", file=sys.stderr)
        sys.exit(1)
