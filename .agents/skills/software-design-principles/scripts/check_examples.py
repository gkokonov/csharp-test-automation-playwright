#!/usr/bin/env python3
"""Compile and run the marked Markdown examples in references/.

Every fenced C#, Java, or TypeScript block carries a marker on the line above:

    <!-- example: <id> | verify: <tokens> -->

Tokens are comma-separated:
  compile            build or type-check the block exactly as written
  run                execute it and compare stdout with EXPECTED_STDOUT
  contract:<name>    append a probe from CONTRACTS and run that too
  none (<reason>)    deliberately unchecked; the reason is reported

The Markdown declares *what* to check and this script owns *how*, so blocks can
be reordered, added, or removed without silently losing coverage. An unmarked
block, an unknown token, an unknown contract, a duplicate id, or a missing
REQUIRED_IDS entry all fail the run.

Uses installed Python 3 plus the toolchain for whichever languages are checked.
It installs no toolchains or third-party packages; SDK restore can still need
targeting packs or package-source access. Use --only to restrict languages and
--node/--tsc-js/--dotnet/--javac/--java-bin for tools outside PATH. Node/TypeScript
calls to Windows executables from WSL need --work-dir on a Windows-mounted drive;
use native toolchains and paths for C# and Java.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import tempfile

MARKED = re.compile(
    r"<!--\s*example:\s*(?P<id>[\w-]+)\s*\|\s*verify:\s*(?P<verify>.*?)\s*-->\s*\n"
    r"```(?P<fence>csharp|java|typescript)\n(?P<code>.*?)```",
    re.S,
)

LANGUAGES = {
    "csharp": {"fence": "csharp", "glob": "csharp*.md"},
    "java": {"fence": "java", "glob": "java*.md"},
    "typescript": {"fence": "typescript", "glob": "typescript*.md"},
}

# Examples that must stay present; guards accidental deletion or renaming.
# Keyed by language so --only still enforces the subset it covers. Covers the
# `verify: none` test-automation blocks too: they are never compiled, so this
# presence check is the only guard they have.
REQUIRED_IDS = {
    "registration-before": "csharp",
    "registration-after": "csharp",
    "nunit-before": "csharp",
    "nunit-after": "csharp",
    "birds-before": "java",
    "birds-after": "java",
    "junit-before": "java",
    "junit-after": "java",
    "notification-before": "typescript",
    "notification-after": "typescript",
    "fluent-query": "typescript",
    "playwright-before": "typescript",
    "playwright-after": "typescript",
}

EXPECTED_STDOUT = {
    "registration-before": "Welcome user@example.com",
    "registration-after": "Welcome user@example.com",
    "birds-before": "Eagle flies",
    "birds-after": "Penguin lays an egg\nEagle flies",
    "notification-before": "email to user@example.com: Welcome",
    "notification-after": "email to user@example.com: Welcome",
    "fluent-query": "active = true ORDER BY name",
}

PREFIX = '''const observed: string[] = [];
console.log = (...values: unknown[]) => { observed.push(values.join(" ")); };
'''

CONTRACTS = {
    # Selected-channel delivery and unswallowed provider errors.
    "notification": '''
observed.length = 0;
const compatible = new NotificationService();
const probeMessage = { recipient: "probe@example.com", body: "Probe" };
compatible.notify("email", probeMessage);
compatible.notify("sms", probeMessage);
const expected = ["email to probe@example.com: Probe", "sms to probe@example.com: Probe"];
if (JSON.stringify(observed) !== JSON.stringify(expected)) {
  throw new Error("Selected-channel delivery changed: " + JSON.stringify(observed));
}
const providerFailure = new Error("provider failed");
console.log = () => { throw providerFailure; };
for (const channel of ["email", "sms"] as const) {
  let caught: unknown;
  try { compatible.notify(channel, probeMessage); } catch (error) { caught = error; }
  if (caught !== providerFailure) throw new Error("Provider error was swallowed or replaced");
}
''',
    # Injected providers, message identity, and no fallback delivery.
    # Depends on names declared by the "notification" contract; list it after.
    "injection": '''
const calls: string[] = [];
const injected = new NotificationService(
  { send(message) { if (message !== probeMessage) throw new Error("Message replaced"); calls.push("email"); } },
  { send(message) { if (message !== probeMessage) throw new Error("Message replaced"); calls.push("sms"); } },
);
injected.notify("sms", probeMessage);
injected.notify("email", probeMessage);
if (JSON.stringify(calls) !== '["sms","email"]') throw new Error("Injected channel selection changed");
for (const channel of ["email", "sms"] as const) {
  let otherCalls = 0;
  const failing = { send(_message: Message) { throw providerFailure; } };
  const other = { send(_message: Message) { otherCalls++; } };
  const subject = channel === "email"
    ? new NotificationService(failing, other)
    : new NotificationService(other, failing);
  let caught: unknown;
  try { subject.notify(channel, probeMessage); } catch (error) { caught = error; }
  if (caught !== providerFailure || otherCalls !== 0) throw new Error("Error propagation or fallback changed");
}
''',
}

CSPROJ = """<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>{framework}</TargetFramework>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
"""


def run(command, timeout=300):
    result = subprocess.run(command, capture_output=True, text=True, timeout=timeout)
    if result.returncode:
        raise RuntimeError(f"Command failed ({result.returncode}): {command}\n"
                           f"{result.stdout}\n{result.stderr}")
    return result.stdout.strip()


def native(path, executable):
    """Translate a WSL path for a Windows-hosted tool."""
    path = str(Path(path).resolve())
    if os.name != "nt" and str(executable).lower().endswith(".exe"):
        return run(["wslpath", "-w", path])
    return path


def parse_examples(text, source_name):
    marked = list(MARKED.finditer(text))
    fences = len(re.findall(r"^```(?:csharp|java|typescript)$", text, re.M))
    if len(marked) != fences:
        raise RuntimeError(
            f"{fences - len(marked)} of {fences} code block(s) in {source_name} lack an "
            "`<!-- example: <id> | verify: ... -->` marker directly above the fence")
    examples = []
    for match in marked:
        identifier = match.group("id")
        # Split on commas outside parentheses so a "none (a, b)" reason stays intact.
        tokens = [t.strip() for t in re.split(r",(?![^(]*\))", match.group("verify"))
                  if t.strip()]
        contracts, skip_reason = [], None
        for token in tokens:
            if token in ("compile", "run"):
                continue
            if token.startswith("contract:"):
                name = token.split(":", 1)[1]
                if name not in CONTRACTS:
                    raise RuntimeError(f"{identifier}: unknown contract {name!r}; "
                                       f"known: {sorted(CONTRACTS)}")
                contracts.append(name)
                continue
            if token.startswith("none"):
                skip_reason = token
                continue
            raise RuntimeError(f"{identifier}: unknown verify token {token!r}")
        examples.append({"id": identifier, "source": source_name,
                         "language": match.group("fence"), "tokens": tokens,
                         "contracts": contracts, "skip_reason": skip_reason,
                         "code": match.group("code")})
    return examples


class Runner:
    """Compiles and runs one example, per language."""

    def __init__(self, args):
        self.args = args
        self.tsc = ([args.node, native(args.tsc_js, args.node)] if args.tsc_js
                    else [args.tsc])
        self.tsc_host = args.node if args.tsc_js else args.tsc

    def typescript(self, folder, name, code):
        path = folder / f"{name}.ts"
        path.write_text(code, encoding="utf-8")
        run(self.tsc + [native(path, self.tsc_host), "--strict", "--target", "ES2020",
                        "--outDir", native(folder, self.tsc_host)])
        return lambda: run([self.args.node, native(path.with_suffix(".js"), self.args.node)])

    def csharp(self, folder, name, code):
        project = folder / name
        project.mkdir(parents=True, exist_ok=True)
        (project / "Program.cs").write_text(code, encoding="utf-8")
        (project / f"{name}.csproj").write_text(
            CSPROJ.format(framework=self.args.framework), encoding="utf-8")
        run([self.args.dotnet, "build", str(project), "-v", "q", "--nologo"])
        return lambda: run([self.args.dotnet, "run", "--project", str(project),
                            "--no-build", "-v", "q", "--nologo"])

    def java(self, folder, name, code):
        match = re.search(r"(?:public\s+)?(?:final\s+)?class\s+(\w+)", code)
        if not match:
            raise RuntimeError(f"{name}: no class declaration found")
        entry = match.group(1)
        project = folder / name
        project.mkdir(parents=True, exist_ok=True)
        (project / f"{entry}.java").write_text(code, encoding="utf-8")
        run([self.args.javac, "-Xlint:all", "-Werror", "-d", str(project),
             str(project / f"{entry}.java")])
        return lambda: run([self.args.java_bin, "-cp", str(project), entry])


def main():
    parser = argparse.ArgumentParser(description=__doc__,
                                     formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--only", action="append", choices=sorted(LANGUAGES),
                        help="check only these languages (repeatable)")
    parser.add_argument("--node", default="node")
    parser.add_argument("--tsc", default="tsc")
    parser.add_argument("--tsc-js", type=Path)
    parser.add_argument("--dotnet", default="dotnet")
    parser.add_argument("--javac", default="javac")
    parser.add_argument("--java-bin", default="java")
    parser.add_argument("--framework", default="net10.0")
    parser.add_argument("--work-dir", type=Path)
    args = parser.parse_args()

    references = Path(__file__).resolve().parents[1] / "references"
    selected = args.only or sorted(LANGUAGES)
    sources, examples = [], []
    for language in selected:
        for source in sorted(references.glob(LANGUAGES[language]["glob"])):
            if source in sources:
                continue
            sources.append(source)
            examples += parse_examples(source.read_text(encoding="utf-8"), source.name)
    examples = [e for e in examples if e["language"] in selected]
    if not examples:
        raise RuntimeError(f"No marked examples found for {selected}")

    ids = [e["id"] for e in examples]
    duplicates = sorted({i for i in ids if ids.count(i) > 1})
    if duplicates:
        raise RuntimeError(f"Duplicate example ids: {duplicates}")
    required = {i for i, language in REQUIRED_IDS.items() if language in selected}
    missing = sorted(required - set(ids))
    if missing:
        raise RuntimeError(f"Required examples missing from references/: {missing}")

    runner = Runner(args)
    results, versions = [], {}
    with tempfile.TemporaryDirectory(prefix="skill-examples-", dir=args.work_dir) as temp:
        folder = Path(temp)
        for example in examples:
            identifier, language = example["id"], example["language"]
            record = {"example": identifier, "source": example["source"],
                      "language": language}
            if example["skip_reason"]:
                results.append({**record, "check": "skipped", "result": "skipped",
                                "reason": example["skip_reason"]})
                continue
            build = getattr(runner, language)
            execute = build(folder, f"{identifier}-example", example["code"])
            check = {**record, "check": "example", "result": "passed"}
            if "run" in example["tokens"]:
                output = execute()
                expected = EXPECTED_STDOUT.get(identifier)
                if expected is not None and output != expected:
                    raise RuntimeError(
                        f"{identifier}: expected stdout {expected!r}, got {output!r}")
                check["stdout"] = output
            results.append(check)
            appended = ""
            for name in example["contracts"]:
                appended += CONTRACTS[name]
                probe = build(folder, f"{identifier}-contract-{name}",
                              PREFIX + example["code"] + appended)
                if probe() != "":
                    raise RuntimeError(f"{identifier}: contract {name} produced output")
                results.append({**record, "check": f"contract:{name}", "result": "passed"})

    if "typescript" in selected:
        versions["node"] = run([args.node, "--version"])
        versions["typescript"] = run(runner.tsc + ["--version"])
    if "csharp" in selected:
        versions["dotnet"] = run([args.dotnet, "--version"])
    if "java" in selected:
        versions["javac"] = run([args.javac, "--version"])
    print(json.dumps({"sources": {s.name: hashlib.sha256(s.read_bytes()).hexdigest()
                                  for s in sources},
                      "versions": versions, "checks": results}, indent=2))


if __name__ == "__main__":
    main()
