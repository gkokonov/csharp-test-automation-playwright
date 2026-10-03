# Graphify with Codex on Windows

Read this reference when the active shell is PowerShell. Keep the extraction,
cache, merge, and integrity rules in `SKILL.md`; only adapt shell and host tools.
Use the repository root as the working directory unless the request specifies a
different output location. Do not enable hooks or install additional packages
as part of a query.

## Prefer native CLI commands

These commands work in PowerShell without Bash. A query against an existing
graph does not need interpreter sidecars or a rebuild. Keep the vocabulary
expansion and traversal rules in `references/query.md`.

```powershell
Test-Path -LiteralPath 'graphify-out/graph.json'
graphify query "Shared Agent Instruction Discovery" --budget 1500
graphify path "Shared Agent Instruction Discovery" "Coding Agent Support"
graphify explain "Shared Agent Instruction Discovery"
```

For AST-only code updates, use `graphify update .`. For a requested full or
semantic rebuild, follow the skill's detection and extraction pipeline; an
AST-only update does not extract changed documentation. Export commands such
as `graphify export html` also work directly in PowerShell.

## Resolve the Python interpreter for pipeline blocks

Use the saved interpreter only if it still exists and can import Graphify.
Otherwise try the installed uv tool interpreter, then Python on `PATH`. Do not
assume a `python3` command, a Unix shebang, or a fixed user profile path.

```powershell
$graphifyCandidates = @()
if (Test-Path -LiteralPath 'graphify-out/.graphify_python' -PathType Leaf) {
    $graphifyCandidates += (Get-Content -LiteralPath 'graphify-out/.graphify_python' -Raw).Trim()
}
if (Get-Command uv -ErrorAction SilentlyContinue) {
    $graphifyToolRoot = (& uv tool dir 2>$null | Out-String).Trim()
    if ($LASTEXITCODE -eq 0 -and $graphifyToolRoot) {
        $graphifyCandidates += Join-Path $graphifyToolRoot 'graphifyy/Scripts/python.exe'
    }
}
$graphifyCandidates += Get-Command python.exe, python3.exe -CommandType Application -ErrorAction SilentlyContinue |
    Select-Object -ExpandProperty Source
$graphifyPython = $null
foreach ($graphifyCandidate in ($graphifyCandidates | Select-Object -Unique)) {
    if (-not (Test-Path -LiteralPath $graphifyCandidate -PathType Leaf)) { continue }
    $graphifyResolved = & $graphifyCandidate -c 'import graphify, sys; print(sys.executable)' 2>$null
    if ($LASTEXITCODE -eq 0) {
        $graphifyPython = ($graphifyResolved | Out-String).Trim()
        break
    }
}
if (-not $graphifyPython) {
    throw 'No Python interpreter with graphify is installed. Use the skill installation step before a build.'
}
```

For a build, use the resolved interpreter to create the two sidecars from Step

1. This avoids PowerShell 5.1's default file encoding and passes the scan path
as data. Record the requested scan root, not a different output directory.

```powershell
$graphifyRoot = (Resolve-Path -LiteralPath '.').Path
$graphifySetup = @'
from pathlib import Path
import sys
out = Path("graphify-out")
out.mkdir(exist_ok=True)
(out / ".graphify_python").write_text(sys.executable, encoding="utf-8")
(out / ".graphify_root").write_text(str(Path(sys.argv[1]).resolve()), encoding="utf-8")
'@
$graphifySavedEncoding = $OutputEncoding
try {
    $OutputEncoding = [System.Text.UTF8Encoding]::new($false)
    $graphifySetup | & $graphifyPython - $graphifyRoot
    if ($LASTEXITCODE -ne 0) { throw 'Graphify interpreter sidecar setup failed.' }
}
finally {
    $OutputEncoding = $graphifySavedEncoding
}
```

## Execute multiline Python without shell expansion

Copy the Python body from the relevant pipeline block into a single-quoted
PowerShell here-string. Remove Bash's outer `python -c "..."` wrapper and its
shell-only quote escaping: for example, `\"utf-8\"` becomes `"utf-8"`.
Pass paths through Python's arguments rather than interpolating them into the
source. Use this pattern for detection, extraction, caching, merge, validation,
and reporting. It also works for the inline traversal fallback.

```powershell
$graphifyCode = @'
import json
import sys
from pathlib import Path
from graphify.detect import detect

result = detect(Path(sys.argv[1]))
Path("graphify-out/.graphify_detect.json").write_text(
    json.dumps(result, ensure_ascii=False), encoding="utf-8"
)
print(f"Detected {result['total_files']} files")
'@
$graphifySavedEncoding = $OutputEncoding
try {
    $OutputEncoding = [System.Text.UTF8Encoding]::new($false)
    $graphifyCode | & $graphifyPython - $graphifyRoot
    if ($LASTEXITCODE -ne 0) { throw 'Graphify Python step failed.' }
}
finally {
    $OutputEncoding = $graphifySavedEncoding
}
```

The here-string preserves Python quotes, `$`, and backticks. Set `INPUT_PATH`
and `SPEC_PATH` from `sys.argv` in copied Python bodies. Set `IS_DIRECTED` to
the requested Python boolean, and supply the chosen labels where required.
If a Python body itself needs stdin data, pass that data as an argument or
file instead: stdin in this pattern carries the Python source.

## Semantic chunk files and host tools

Load `references/extraction-spec.md` only when semantic work is needed. Use
the available Codex delegation tools when permitted, with write-capable
workers and bounded concurrency. Otherwise extract each uncached chunk in
the current session. Keep the same prompt, JSON schema, absolute source paths,
chunk validation, and cache attribution in both modes.

Derive each chunk output path from the current output workspace:

```powershell
$graphifyOutput = Join-Path (Get-Location).Path 'graphify-out'
$graphifyChunkNumber = 1
$graphifyChunkPath = Join-Path $graphifyOutput ('.graphify_chunk_{0:D2}.json' -f $graphifyChunkNumber)
```

Use Codex's patch or command tool to write JSON as UTF-8. Do not require a
Claude `Task`/`Write` tool or a particular agent-type label. Record measured
token usage only when the host supplies it. If it does not, label placeholder
counts as unavailable in the generated report and the final response.

## Cleanup after a completed build

Use Python `Path.unlink(missing_ok=True)` from the pipeline or native
PowerShell file operations. Remove only the temporary files named by the
cleanup step. Keep the graph, report, interpreter, root, manifest, and cost
files. Do not feed discovered paths into `cmd /c` or recursively delete the
output directory.

```powershell
$graphifyOutput = (Resolve-Path -LiteralPath 'graphify-out').Path
$graphifyTemporaryNames = @(
    '.graphify_detect.json', '.graphify_extract.json', '.graphify_ast.json',
    '.graphify_semantic.json', '.graphify_analysis.json', '.needs_update'
)
foreach ($graphifyTemporaryName in $graphifyTemporaryNames) {
    $graphifyTemporaryPath = Join-Path $graphifyOutput $graphifyTemporaryName
    if (Test-Path -LiteralPath $graphifyTemporaryPath -PathType Leaf) {
        Remove-Item -LiteralPath $graphifyTemporaryPath
    }
}
Get-ChildItem -LiteralPath $graphifyOutput -Filter '.graphify_chunk_*.json' -File |
    ForEach-Object { Remove-Item -LiteralPath $_.FullName }
```
