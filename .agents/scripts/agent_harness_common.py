"""Checked local commands and disposable repositories for harness validation."""

import contextlib
import json
import os
from pathlib import Path
import shutil
import stat
import subprocess
import tempfile


def require(condition, message):
    if not condition:
        raise RuntimeError(message)


def run(command, cwd, *, expected=(0,), timeout=60):
    result = subprocess.run(command, cwd=cwd, capture_output=True, text=True,
                            encoding="utf-8", errors="replace", timeout=timeout)
    require(result.returncode in expected,
            f"Command failed ({result.returncode}): {command!r}\n{result.stderr}\n{result.stdout}")
    return result


def ocr_command():
    executable = shutil.which("ocr")
    require(executable, "OCR is unavailable on PATH.")
    path = Path(executable)
    if os.name == "nt" and path.suffix.lower() in {".cmd", ".bat", ".ps1"}:
        wrapper = path.parent / "node_modules/@alibaba-group/open-code-review/bin/ocr.js"
        node = shutil.which("node")
        require(node and wrapper.is_file(),
                "Use the OCR npm installation or put the OCR native executable on PATH.")
        return [node, str(wrapper)]
    return [executable]


def ocr(repo, *arguments):
    return json.loads(run([*ocr_command(), "delegate", *arguments,
                           "--repo", str(repo), "--format", "json"], repo).stdout)


def write(path, content):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content, encoding="utf-8", newline="\n")


@contextlib.contextmanager
def disposable_repository(prefix="agent-harness-"):
    temp_root = Path(tempfile.gettempdir()).resolve()
    repository = Path(tempfile.mkdtemp(prefix=prefix)).resolve()
    require(repository.parent == temp_root and repository.name.startswith(prefix),
            "Fixture must be a unique direct child of the temporary directory.")
    try:
        run(["git", "init", "--quiet"], repository)
        for key, value in (("user.name", "Harness Fixture"),
                           ("user.email", "fixture@example.invalid"),
                           ("commit.gpgsign", "false"), ("core.autocrlf", "false")):
            run(["git", "config", "--local", key, value], repository)
        yield repository
    finally:
        # Check the absolute target immediately before recursive cleanup.
        require(repository.resolve() == repository and repository.parent == temp_root
                and repository.name.startswith(prefix), "Unsafe fixture cleanup target.")
        def remove_readonly(function, path, error):
            target = Path(path).resolve()
            require(target.is_relative_to(repository), "Cleanup path escaped the fixture.")
            if not isinstance(error, PermissionError):
                raise error
            target.chmod(stat.S_IWRITE | stat.S_IREAD)
            function(path)

        shutil.rmtree(repository, onexc=remove_readonly)
