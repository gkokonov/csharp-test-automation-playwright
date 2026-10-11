[CmdletBinding()]
param([switch] $OcrFixtures)

$ErrorActionPreference = 'Stop'
$harnessRepository = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../..')).Path
Push-Location -LiteralPath $harnessRepository
try {
    $instructionRoot = (Resolve-Path -LiteralPath '.').Path
    $instructionRootFile = Join-Path $instructionRoot 'AGENTS.md'
    $instructionLineCount = [System.IO.File]::ReadAllLines($instructionRootFile).Count
    $instructionBytes = (Get-Item -LiteralPath $instructionRootFile).Length
    Write-Output "Root AGENTS.md: $instructionLineCount lines; $instructionBytes bytes"
    if ($instructionLineCount -gt 100) {
        throw "Root AGENTS.md exceeds the 100-line maximum: $instructionLineCount"
    }
    if ($instructionBytes -gt 5KB) {
        Write-Warning 'Root AGENTS.md exceeds the recommended 5 KiB context budget.'
    }

    $instructionFiles = @(
        Get-Item -LiteralPath 'AGENTS.md',
        'CsharpTestAutomation.Framework/AGENTS.md',
        'CsharpTestAutomation.Tests/AGENTS.md',
        'CsharpTestAutomation.Bdd.Tests/AGENTS.md',
        'CsharpTestAutomation.Framework.Test/AGENTS.md',
        'README.md', 'docs/AGENT_SUPPORT.md',
        'docs/API_TESTING_ARCHITECTURE.md', 'docs/UI_TESTING_ARCHITECTURE.md',
        'docs/netbox-automation/03-device-management-spec.md'
        Get-ChildItem -LiteralPath '.agents/rules', '.agents/instructions',
        '.github/instructions', '.agents/implementation', '.agents/review',
        '.agents/review/rules', '.agents/agents', '.github/agents' -Filter '*.md' -File
    )
    $instructionMissing = [System.Collections.Generic.List[string]]::new()
    $instructionLinksChecked = 0
    function Get-InstructionAnchors([string] $path) {
        $anchors = [System.Collections.Generic.HashSet[string]]::new()
        $duplicates = @{}
        $fenced = $false
        foreach ($line in [System.IO.File]::ReadAllLines($path)) {
            if ($line -match '^\s*(```|~~~)') { $fenced = -not $fenced; continue }
            if ($fenced -or $line -notmatch '^#{1,6}\s+(.+?)\s*#*\s*$') { continue }
            $slug = [regex]::Replace($Matches[1].ToLowerInvariant(), '[^\p{L}\p{N}_\- ]', '').Replace(' ', '-')
            $anchor = $slug
            if ($duplicates.ContainsKey($slug)) { $duplicates[$slug]++; $anchor = "$slug-$($duplicates[$slug])" }
            else { $duplicates[$slug] = 0 }
            [void]$anchors.Add($anchor)
        }
        return , $anchors
    }
    foreach ($instructionFile in $instructionFiles) {
        $instructionText = Get-Content -Encoding UTF8 -Raw -LiteralPath $instructionFile.FullName
        foreach ($instructionMatch in [regex]::Matches($instructionText, '\[[^\]\r\n]+\]\(([^)\r\n]+)\)')) {
            $instructionTarget = $instructionMatch.Groups[1].Value.Trim().Trim([char[]]'<>')
            if ($instructionTarget -match '^[a-zA-Z][a-zA-Z0-9+.-]*:') { continue }
            $instructionParts = $instructionTarget -split '#', 2
            $instructionFileTarget = [Uri]::UnescapeDataString($instructionParts[0])
            $instructionResolved = if ($instructionFileTarget) {
                Join-Path $instructionFile.DirectoryName $instructionFileTarget
            }
            else { $instructionFile.FullName }
            $instructionLinksChecked++
            if (-not (Test-Path -LiteralPath $instructionResolved -PathType Leaf)) {
                $instructionMissing.Add("$($instructionFile.FullName) -> $instructionTarget")
                continue
            }
            if ($instructionParts.Count -eq 2 -and $instructionParts[1]) {
                $instructionAnchors = Get-InstructionAnchors $instructionResolved
                if (-not $instructionAnchors.Contains([Uri]::UnescapeDataString($instructionParts[1]))) {
                    $instructionMissing.Add("$($instructionFile.FullName) -> $instructionTarget (fragment)")
                }
            }
        }
    }
    if ($instructionMissing.Count -gt 0) {
        throw ("Broken local links:`n" + ($instructionMissing -join "`n"))
    }
    Write-Output "Checked $instructionLinksChecked local links in $($instructionFiles.Count) files."

    $instructionGlobs = @{}
    foreach ($instructionPair in @(
            @('test-automation', 'application-tests'), @('api-testing', 'api-testing'),
            @('ui-testing', 'ui-testing'), @('bdd-testing', 'bdd-testing')
        )) {
        $instructionRule = Get-Content -Raw -LiteralPath ".agents/rules/$($instructionPair[0]).md"
        $instructionAdapter = Get-Content -Raw -LiteralPath ".github/instructions/$($instructionPair[1]).instructions.md"
        $instructionPatterns = @([regex]::Match($instructionRule, '(?m)^globs: "([^"]+)"').Groups[1].Value.Split(',').Trim())
        $instructionApply = @([regex]::Match($instructionRule, '(?m)^applyTo: "([^"]+)"').Groups[1].Value.Split(',').Trim())
        $instructionCopilot = @([regex]::Match($instructionAdapter, '(?m)^applyTo: "([^"]+)"').Groups[1].Value.Split(',').Trim())
        if (-not $instructionPatterns[0] -or
            (Compare-Object $instructionPatterns $instructionApply) -or
            (Compare-Object $instructionPatterns $instructionCopilot)) {
            throw "Canonical/adapter glob mismatch: $($instructionPair[0])"
        }
        $instructionGlobs[$instructionPair[0]] = $instructionPatterns
    }
    function Test-InstructionGlob([string] $path, [string[]] $patterns) {
        foreach ($pattern in $patterns) {
            $regex = '^' + [regex]::Escape($pattern).Replace('\*\*/', '(?:.*/)?').Replace('\*', '[^/]*') + '$'
            if ([regex]::IsMatch($path, $regex)) { return $true }
        }
        return $false
    }
    # Columns: path, shared application rules, API rules, UI rules, BDD rules.
    $instructionCases = @(
        @('CsharpTestAutomation.Tests/API/Clients/SitesApiClient.cs', $true, $true, $false, $false),
        @('CsharpTestAutomation.Tests/Tests/ApiTestBase.cs', $true, $true, $false, $false),
        @('CsharpTestAutomation.Tests/Tests/API/SitesApiTests.cs', $true, $true, $false, $false),
        @('CsharpTestAutomation.Tests/UI/Pages/NetBox/SitesListPage.cs', $true, $false, $true, $false),
        @('CsharpTestAutomation.Tests/Tests/UiTestBase.cs', $true, $false, $true, $false),
        @('CsharpTestAutomation.Tests/Tests/UI/NetBox/SiteManagementUiTests.cs', $true, $false, $true, $false),
        @('CsharpTestAutomation.Bdd.Tests/API/Clients/SitesApiClient.cs', $true, $true, $false, $true),
        @('CsharpTestAutomation.Bdd.Tests/Steps/API/NetBox/SiteApiSteps.cs', $true, $true, $false, $true),
        @('CsharpTestAutomation.Bdd.Tests/UI/Pages/NetBox/SitesListPage.cs', $true, $false, $true, $true),
        @('CsharpTestAutomation.Bdd.Tests/Steps/UI/NetBox/SiteUiSteps.cs', $true, $false, $true, $true),
        @('CsharpTestAutomation.Bdd.Tests/Features/SiteApi.feature', $false, $true, $false, $true),
        @('CsharpTestAutomation.Bdd.Tests/Features/SiteUi.feature', $false, $false, $true, $true),
        @('CsharpTestAutomation.Bdd.Tests/Hooks/ScenarioHooks.cs', $true, $false, $false, $true),
        @('CsharpTestAutomation.Bdd.Tests/Context/SiteScenarioState.cs', $true, $false, $false, $true),
        @('CsharpTestAutomation.Bdd.Tests/AssemblyInfo.cs', $true, $false, $false, $true),
        @('CsharpTestAutomation.Bdd.Tests/reqnroll.json', $false, $false, $false, $true),
        @('CsharpTestAutomation.Framework/API/Clients/RestClientFactory.cs', $false, $false, $false, $false),
        @('CsharpTestAutomation.Framework.Test/AssemblyInfo.cs', $false, $false, $false, $false)
    )
    foreach ($instructionCase in $instructionCases) {
        $instructionIndex = 1
        foreach ($instructionRuleName in @('test-automation', 'api-testing', 'ui-testing', 'bdd-testing')) {
            if ((Test-InstructionGlob $instructionCase[0] $instructionGlobs[$instructionRuleName]) -ne $instructionCase[$instructionIndex]) {
                throw "Unexpected routing: $($instructionCase[0]) -> $instructionRuleName"
            }
            $instructionIndex++
        }
    }
    Write-Output "Checked $($instructionCases.Count) path routes and four canonical/adapter glob pairs."
    git diff --check
    if ($LASTEXITCODE -ne 0) { throw 'git diff --check failed.' }
    python (Join-Path $PSScriptRoot 'check_agent_configuration.py') --repo $harnessRepository
    if ($LASTEXITCODE -ne 0) { throw 'Agent configuration checks failed.' }
    if ($OcrFixtures) {
        python (Join-Path $PSScriptRoot 'check_ocr_fixtures.py') --repo $harnessRepository
        if ($LASTEXITCODE -ne 0) { throw 'OCR fixture checks failed.' }
    }
}
finally {
    Pop-Location
}
